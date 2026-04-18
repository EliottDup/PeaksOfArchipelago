using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using Rewired;
using UnityEngine;
using Player = Rewired.Player;

namespace PeaksOfArchipelago.MonoBehaviours
{
    internal class PeakJournal
    {
        internal struct PeakPage
        {
            public Peaks peak;
            public Texture pageTex;
            public Texture pageTexUnStamped;
            public Texture pageTexStamped;
            public Texture pageTexFS;
            public bool hasFS;
            public int sceneIndex;
        }

        private Transform cameraPosition;

        // Selection ouitlines & colliders
        public GameObject leftSelectOutlineOBJ;
        public GameObject rightSelectOutlineOBJ;
        public Collider leftPageCol;
        public Collider rightPageCol;

        public Collider bookInteractCollider;
        public Collider backCollider; // don't really know what to do with this


        public const int leftpageTextureOffset = 0;
        public const int rightpageTextureOffset = 3;
        public const int frontpageTextureOffset = 1;
        public const int backpageTextureOffset = 2;



        // materials
        public Material leftPage;
        public Material rightPage;
        public Material frontPage;
        public Material backPage;
        public Material peakjournalMaterial;


        // Audio
        public AudioSource scootchSound;
        public AudioSource journalSound;

        // Animations
        private Animation bookBinding;
        private Animation pageHeightAdjust;
        private Animation journalPageAnim;

        // Map
        public Animator spriteWalkerAnim;

        public Animation mapjournalControllerAnim;
        public Animation mapAnim;

        public ParticleSystem smokePuff;
        public ParticleSystem foostepParticle;
        
        public SpriteRenderer fellWalkerSprite;
        
        public Transform fellwalkerSpriteTr;
        public Transform footstepTr;

        public Transform[] mapPeaks;
        public TextMesh[] mapPeakNames;

        // Misc
        public GameObject closeJournalTextObj;

        public PeakPage emptyPage;
        public PeakPage startPage;
        private Color skyReadingColor;
        static Color defaultSkyColor;

        public int currentPage = 0;

        List<PeakPage> pages;

        PlayerManager playerManager;
        Player player;

        private bool playingIntro = false;
        private bool canInteract = true;

        private bool inJournal;

        private int journalNum = 1;

        private ISlotData slotData;

        bool playerPress;
        bool bookIsOpen;

        GameObject camera;

        Camera MainCam;
        Camera JournalCam;

        public PlayerMouseSprite gamepadMouse;

        public void Initialise()
        {
            // DO SHTUFF
        }

        public void MyStart()
        {
            slotData = Connection.Instance.slotData;
            playingIntro = false;
            canInteract = true;
            LoadPagesData();
            UpdatePages(currentPage);
            bookBinding["bookbinding_close"].normalizedTime = 1f;
        }

        private void UpdatePages(int page)
        {
            PeakPage[] enabledPages = GetEnabledPeaks();
            leftPage.mainTexture = GetPage(enabledPages, page + leftpageTextureOffset).pageTex;
            rightPage.mainTexture = GetPage(enabledPages, page + rightpageTextureOffset).pageTex;
            frontPage.mainTexture = GetPage(enabledPages, page + frontpageTextureOffset).pageTex;
            backPage.mainTexture = GetPage(enabledPages, page + backpageTextureOffset).pageTex;
        }

        public void LoadPagesData()
        {
            if (PlayerPrefs.HasKey($"PeakJournal{journalNum}_Alps_CurrentPage"))
            {
                currentPage = PlayerPrefs.GetInt($"PeakJournal{journalNum}_Alps_CurrentPage", currentPage);
            }
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i] = UpdatePeakPage(pages[i]);
            }
        }
        
        public PeakPage UpdatePeakPage(PeakPage page)
        {
            if (Connection.Instance.HasLocation(LocationIDs.GetPeakLocationID(page.peak)))
            {
                if (Connection.Instance.HasLocation(LocationIDs.GetFSPeakLocationID(page.peak))) 
                {
                    page.pageTex = page.pageTexFS;
                }
                else
                {
                    page.pageTex = page.pageTexStamped;
                }
            }
            else
            {
                page.pageTex = page.pageTexUnStamped;
            }
            return page;
        }

        private PeakPage GetPage(PeakPage[] pages, int page)
        {
            if (page == 0)
            {
                return startPage;
            }
            if (page < 0 || page > pages.Length)
            {
                return emptyPage;
            }

            return pages[page-1];
        }


        public PeakPage[] GetEnabledPeaks()
        {
            return pages.Where(page => slotData.HasPeak(page.peak)).ToArray();
        }

        public void Update()
        {
            playerPress = player.GetButton("Arm Left");

            if (bookIsOpen)
            {
                if (!inJournal)
                {
                    inJournal = true;
                    gamepadMouse.mouse.screenPosition = new Vector2(Screen.width / 2, Screen.height / 2);
                }

                camera.SetActive(true);


                //if (Physics.Raycast(JournalCam.ScreenPointToRay(gamepadMouse.mouse.screenPosition), out RaycastHit rayHit, )
            }

        }
    }
}
