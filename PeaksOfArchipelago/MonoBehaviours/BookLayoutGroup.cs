using UnityEngine.UI;
using UnityEngine;

namespace PeaksOfArchipelago.MonoBehaviours
{
    public class BookLayoutGroup : GridLayoutGroup
    {

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            if (axis == 0)
            {
                for (int i = 0; i < base.rectChildren.Count; i++)
                {
                    RectTransform rectTransform = base.rectChildren[i];
                    m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
                    rectTransform.anchorMin = Vector2.up;
                    rectTransform.anchorMax = Vector2.up;
                    rectTransform.sizeDelta = cellSize;
                }

                return;
            }

            float x = base.rectTransform.rect.size.x;
            float y = base.rectTransform.rect.size.y;
            int maxHoriz = 1;
            int maxVert = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                maxHoriz = m_ConstraintCount;
                if (base.rectChildren.Count > maxHoriz)
                {
                    maxVert = base.rectChildren.Count / maxHoriz + ((base.rectChildren.Count % maxHoriz > 0) ? 1 : 0);
                }
            }
            else if (m_Constraint != Constraint.FixedRowCount)
            {
                maxHoriz = ((!(cellSize.x + spacing.x <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((x - (float)base.padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x))) : int.MaxValue);
                maxVert = ((!(cellSize.y + spacing.y <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((y - (float)base.padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y))) : int.MaxValue);
            }
            else
            {
                maxVert = m_ConstraintCount;
                if (base.rectChildren.Count > maxVert)
                {
                    maxHoriz = base.rectChildren.Count / maxVert + ((base.rectChildren.Count % maxVert > 0) ? 1 : 0);
                }
            }

            int startRight = (int)startCorner % 2;
            int startLower = (int)startCorner / 2;
            int maxInStartDir;
            int maxHorizReach;
            int maxVertReach;
            if (startAxis == Axis.Horizontal)
            {
                maxInStartDir = maxHoriz;
                maxHorizReach = Mathf.Clamp(maxHoriz, 1, base.rectChildren.Count);
                maxVertReach = Mathf.Clamp(maxVert, 1, Mathf.CeilToInt((float)base.rectChildren.Count / (float)maxInStartDir));
            }
            else
            {
                maxInStartDir = maxVert;
                maxVertReach = Mathf.Clamp(maxVert, 1, base.rectChildren.Count);
                maxHorizReach = Mathf.Clamp(maxHoriz, 1, Mathf.CeilToInt((float)base.rectChildren.Count / (float)maxInStartDir));
            }

            Vector2 totalRectSize = new Vector2((float)maxHorizReach * cellSize.x + (float)(maxHorizReach - 1) * spacing.x, (float)maxVertReach * cellSize.y + (float)(maxVertReach - 1) * spacing.y);
            Vector2 startOffsets = new Vector2(GetStartOffset(0, totalRectSize.x), GetStartOffset(1, totalRectSize.y));

            int n = base.rectChildren.Count % maxInStartDir;

            for (int j = 0; j < base.rectChildren.Count; j++)
            {
                int iHoriz;
                int iVert;
                if (startAxis == Axis.Horizontal)
                {
                    iHoriz = j % maxInStartDir;
                    iVert = j / maxInStartDir;
                }
                else
                {
                    iHoriz = j / maxInStartDir;
                    iVert = j % maxInStartDir;
                }

                if (startRight == 1)
                {
                    iHoriz = maxHorizReach - 1 - iHoriz;
                }

                if (startLower == 1)
                {
                    iVert = maxVertReach - 1 - iVert;
                }

                Vector2 size = new Vector2(cellSize.x, cellSize.y);


                if (startAxis == Axis.Horizontal)
                {
                    int div = iHoriz < n ? maxVertReach : maxVertReach - 1;
                    size.y /= n == 0 ? div + 1 : div;
                }
                else
                {
                    int div = Mathf.Max(iVert < n ? maxHorizReach : maxHorizReach - 1, 1);
                    size.x /= div == 0 ? div + 1 : div;
                }

                SetChildAlongAxis(base.rectChildren[j], 0, startOffsets.x + (size[0] + spacing[0]) * (float)iHoriz, size[0]);
                SetChildAlongAxis(base.rectChildren[j], 1, startOffsets.y + (size[1] + spacing[1]) * (float)iVert, size[1]);
            }
        }
    }
}
