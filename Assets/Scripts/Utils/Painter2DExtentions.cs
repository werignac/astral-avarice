using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Utils
{
    public static class Painter2DExtentions
    {
		public static void DrawCircleSlice(this Painter2D painter, float startingAngle, float endingAngle, Vector2 center, float radius)
		{
			painter.BeginPath();
			painter.Arc(center, radius, startingAngle, endingAngle);
			painter.LineTo(center);
			painter.ClosePath();
			painter.Fill();
		}

		public static void DrawCircle(this Painter2D painter, Vector2 center, float radius)
		{
			painter.BeginPath();
			painter.Arc(center, radius, 0f, 360f);
			painter.ClosePath();
			painter.Fill();
		}

		public static void DrawTorusSlice(this Painter2D painter, float startingAngle, float endingAngle, Vector2 center, float outerRadius, float innerRadius)
		{
			float midRadius = (outerRadius + innerRadius) / 2;
			float torusWidth = Mathf.Abs(outerRadius - innerRadius) / 2;

			painter.lineCap = LineCap.Butt;
			painter.lineWidth = torusWidth;

			painter.BeginPath();
			painter.Arc(center, midRadius, startingAngle, endingAngle);
			painter.Stroke();
		}

		// TODO: Get rid of this. Unity gradients are too limiting for this to be useful.
		public static void DottedLine(this Painter2D painter, Color lineColor, float dotToGapAspect, int dotCount)
		{
			Debug.Assert(dotCount <= 4, "Can only have up to 4 dots due to Unity gradient limitations.");

			Gradient dottedLineGradient = new Gradient();
			dottedLineGradient.mode = GradientMode.Fixed;
			dottedLineGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(lineColor, 0) };

			float dotAndSpacePercent = 1 / dotCount;
			float dotPercent = dotAndSpacePercent * (dotToGapAspect / (dotToGapAspect + 1));
			List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();

			for(int i = 0; i < dotCount; i++)
			{
				alphaKeys.Add(new GradientAlphaKey(1, i * dotAndSpacePercent + dotPercent));
				alphaKeys.Add(new GradientAlphaKey(0, (i + 1) * dotAndSpacePercent));
			}

			dottedLineGradient.alphaKeys = alphaKeys.ToArray();

			painter.strokeGradient = dottedLineGradient;
		}
	}
}
