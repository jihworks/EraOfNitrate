// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

using UnityEngine;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public readonly struct HexaOrientation
    {
        public readonly Vector2 Origin, Radius;

        readonly float F0, F1, F2, F3;
        readonly float B0, B1, B2, B3;

        /// <summary>
        /// Horizontal distance from center of a cell to another center of a cell.
        /// </summary>
        public float ScreenHorizontalSpacing => Radius.x * F0;
        /// <summary>
        /// Vertical distance from center of a cell to another center of a cell.
        /// </summary>
        public float ScreenVerticalSpacing => Radius.y * F3;

        /// <param name="origin">A coordinate for center of the origin cell in screen space. Commonly <see cref="Vector2.zero"/>.</param>
        /// <param name="radius">Radius of circumcircle of a cell in screen space. Commonly <see cref="Vector2.one"/>.</param>
        public HexaOrientation(Vector2 origin, Vector2 radius)
            : this(origin, radius,
                  // Pointy-topped hexagon orientation.
                  Mathf.Sqrt(3f), Mathf.Sqrt(3f) / 2f, 0f, 3f / 2f,
                  Mathf.Sqrt(3f) / 3f, -1f / 3f, 0f, 2f / 3f)
        {
        }
        private HexaOrientation(Vector2 origin, Vector2 radius, float f0, float f1, float f2, float f3, float b0, float b1, float b2, float b3)
        {
            Origin = origin;
            Radius = radius;
            F0 = f0;
            F1 = f1;
            F2 = f2;
            F3 = f3;
            B0 = b0;
            B1 = b1;
            B2 = b2;
            B3 = b3;
        }

        public Vector2 HexaToScreen(HexaCoordF h)
        {
            float x = (F0 * h.A + F1 * h.B) * Radius.x;
            float y = (F2 * h.A + F3 * h.B) * Radius.y;
            return new Vector2(x + Origin.x, y + Origin.y);
        }

        public HexaCoordF ScreenToHexa(Vector2 p)
        {
            Vector2 pt = new((p.x - Origin.x) / Radius.x, (p.y - Origin.y) / Radius.y);
            float a = B0 * pt.x + B1 * pt.y;
            float b = B2 * pt.x + B3 * pt.y;
            return new HexaCoordF(a, b, -a - b);
        }
    }
}
