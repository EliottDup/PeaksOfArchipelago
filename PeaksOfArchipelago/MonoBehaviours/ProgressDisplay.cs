using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using BepInEx.Logging;
using PeaksOfArchipelago.Extensions;
using static RootMotion.FinalIK.VRIKCalibrator;


namespace PeaksOfArchipelago.MonoBehaviours
{
    internal class ProgressDisplay : MonoBehaviour
    {
        Connection connection;
        Canvas canvas;
        Transform progressDisplayRoot;
        Dictionary<Books, Transform> books = new Dictionary<Books, Transform>();
        Dictionary<Peaks, Transform> peaks = new Dictionary<Peaks, Transform>();

        ManualLogSource Logger;

        SessionSettings settings;

        private bool visible = false;

        public void Update()
        {
            if (visible)
            {
                if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
                {
                    Disable();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    Enable();
                }

            }
        }

        private void Enable()
        {
            visible = true;
            progressDisplayRoot.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LoadValues(connection.slotData);
        }

        private void Disable()
        {
            visible = false;
            progressDisplayRoot.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        internal void Initialize(Canvas canvas, Connection connection)
        {
            this.canvas = canvas;
            this.connection = connection;
            this.Logger = PeaksOfArchipelago.Logger;
            
            progressDisplayRoot = Instantiate(PeaksOfAssets.ProgressDisplay, canvas.transform).transform;
            progressDisplayRoot.SetAsLastSibling(); // set as last as it should be visible over everything else (even the escape menu apparently whoops)
            progressDisplayRoot.gameObject.SetActive(false);

            Vector2 size = ((RectTransform)canvas.transform).sizeDelta;
            PeaksOfArchipelago.Logger.LogInfo(size);
            BookLayoutGroup layout = progressDisplayRoot.gameObject.AddComponent<BookLayoutGroup>();
            layout.constraint = BookLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.cellSize = new Vector2(size.x / 4, size.y);

            // Somehow get the slotdata here ??
            this.settings = connection.settings;
            InitObjects();
        }

        private void LoadValues(ISlotData data)
        {
            foreach (Books book in Enum.GetValues(typeof(Books)))
            {
                LoadBookData(book, data);
            }
        }

        private void LoadBookData(Books book, ISlotData data)
        {
            bool unlocked = data.HasBook(book);
            books[book].gameObject.SetActive(unlocked);
            if (!unlocked) return;
            Transform nameObject = books[book].FindDeep("BOOKNAME");
            if (nameObject == null)
            {
                Logger.LogError("Couldn't find book name text field");
            }
            nameObject.GetComponent<Text>().text = Mappings.GetBookName(book);
            Peaks[] peaksForBook = [.. Mappings.GetBookPeaks(book)];
            foreach (Peaks peak in peaksForBook)
            {
                LoadPeakData(peak, data);
            }
        }

        private void LoadPeakData(Peaks peak, ISlotData data)
        {
            bool unlocked = data.HasPeak(peak);
            peaks[peak].gameObject.SetActive(unlocked);
            if (!unlocked) return;
            // TODO: Removal of entries

            Text nameText = peaks[peak].FindDeep("PEAKNAME").GetComponent<Text>();
            nameText.text = Mappings.GetPeakName(peak);

            bool peaked = connection.HasLocation(LocationIDs.GetPeakLocationID(peak));
            bool fsComplete = connection.HasLocation(LocationIDs.GetFSPeakLocationID(peak));
            bool timeComplete = connection.HasLocation(LocationIDs.GetTATimePBLocationID(peak));
            bool holdsComplete = connection.HasLocation(LocationIDs.GetTAHoldsLocationID(peak));
            bool ropeComplete = connection.HasLocation(LocationIDs.GetTARopeLocationID(peak));

            peaks[peak].FindDeep("COMPLETIONCHECK")?.gameObject.SetActive(peaked);
            peaks[peak].FindDeep("FREESOLOCHECK")?.gameObject.SetActive(fsComplete);

            CanvasGroup cgTime = peaks[peak].FindDeep("TIMECHECK").gameObject.GetComponent<CanvasGroup>();
            CanvasGroup cgHolds = peaks[peak].FindDeep("HOLDSCHECK").gameObject.GetComponent<CanvasGroup>();
            CanvasGroup cgRopes = peaks[peak].FindDeep("ROPESCHECK").gameObject.GetComponent<CanvasGroup>();

            cgTime.alpha = timeComplete ? 1.0f : 0.0f;
            cgHolds.alpha = holdsComplete ? 1.0f : 0.0f;
            cgRopes.alpha = ropeComplete ? 1.0f : 0.0f;

            long[] collectables = Mappings.GetPeakLocations(peak).ToArray();
            int artefactCount = 0;
            foreach (long location in collectables)
            {
                if (!connection.HasLocation(location))
                {
                    artefactCount++;
                }
            }
                
            peaks[peak].FindDeep("ARTEFACTCOUNT").gameObject.GetComponent<Text>().text = artefactCount.ToString() + "\n";

            bool peakComplete = peaked && artefactCount == 0 &&
                (fsComplete || !Mappings.HasFreeSolo(peak) || !settings.includeFreeSolo) &&
                ((timeComplete && holdsComplete && ropeComplete) || !Mappings.HasTimeAttack(peak) || !settings.includeTimeAttack);

            if (peakComplete)
            {
                nameText.color = Color.green;
            }
        }

        private void InitObjects()
        {
            books = [];
            peaks = [];

            foreach (Transform t in progressDisplayRoot)
            {
                Destroy(t.gameObject);
            }

            foreach (Books book in Enum.GetValues(typeof(Books)))
            {
                CreateBookEntry(book);
            }
        }

        private void CreateBookEntry(Books book)
        {
            Transform bookEntry = Instantiate(PeaksOfAssets.BookEntryPrefab, progressDisplayRoot).transform;
            books.Add(book, bookEntry);

            Transform fsCol = bookEntry.FindDeep("FREESOLO");
            if (fsCol)
            {
                fsCol.gameObject.SetActive(settings.includeFreeSolo);
            }
            else
            {
                Logger.LogWarning("Couldn't find Free Solo column in book entry prefab");
            }
            
            Transform taCol = bookEntry.FindDeep("TIMEATTACK");
            if (taCol)
            {
                taCol.gameObject.SetActive(settings.includeTimeAttack);
            }
            else
            {
                Logger.LogWarning("Couldn't find Time Attack column in book entry prefab");
            }


            Transform peaksParent = bookEntry.FindDeep("ENTRYHOLDER");

            Peaks[] peaksForBook = [.. Mappings.GetBookPeaks(book)];
            foreach (Peaks peak in peaksForBook)
            {
                CreatePeakEntry(peak, peaksParent);
            }
        }

        private void CreatePeakEntry(Peaks peak, Transform parent)
        {
            Transform peakEntry = Instantiate(PeaksOfAssets.PeakEntryPrefab, parent).transform;
            peaks.Add(peak, peakEntry);

            Transform fsCol = peakEntry.FindDeep("FREESOLO");
            if (fsCol)
            {
                fsCol.gameObject.SetActive(settings.includeFreeSolo);
                if (settings.includeFreeSolo)
                {
                    CanvasGroup cg = fsCol.gameObject.GetComponent<CanvasGroup>() ?? fsCol.gameObject.AddComponent<CanvasGroup>();
                    
                    cg.alpha = Mappings.HasFreeSolo(peak) ? 1.0f : 0.0f;
                }
            }
            else
            {
                Logger.LogWarning("Couldn't find Free Solo column in book entry prefab");
            }

            Transform taCol = peakEntry.FindDeep("TIMEATTACK");
            if (taCol)
            {
                taCol.gameObject.SetActive(settings.includeTimeAttack);
                if (settings.includeTimeAttack)
                {
                    CanvasGroup cg = taCol.gameObject.GetComponent<CanvasGroup>() ?? taCol.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = Mappings.HasTimeAttack(peak) ? 1.0f : 0.0f;
                }
            }
            else
            {
                Logger.LogWarning("Couldn't find Time Attack column in book entry prefab");
            }
        }
    }
}
