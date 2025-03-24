using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
    public class IncomePieChartController
    {
		private Color incomeColor = Color.green;
		private Color goalColor = Color.red;
		private Color goalTextColor = Color.white;
		private Color negativeIncomeColor = Color.red;

		private int goal;
		private int income;

		private const string PIE_CHART_ELEMENT_NAME = "PieChart";
		private const string INCOME_LABEL_ELEMENT_NAME = "PieChartIncome";
		private const string GOAL_LABEL_ELEMENT_NAME = "PieChartGoal";

		private VisualElement pieChartElement;
		private Label pieChartIncomeLabel;
		private Label pieChartGoalLabel;

		public void Bind(VisualElement rootElement)
		{
			if (pieChartElement != null)
				pieChartElement.generateVisualContent -= DrawPieChart;

			pieChartElement = rootElement.Q(PIE_CHART_ELEMENT_NAME);
			pieChartElement.generateVisualContent += DrawPieChart;
			pieChartIncomeLabel = rootElement.Q<Label>(INCOME_LABEL_ELEMENT_NAME);
			pieChartGoalLabel = rootElement.Q<Label>(GOAL_LABEL_ELEMENT_NAME);

			Update();
		}

		public void SetIncomeAndGoal(int income, int goal)
		{
			Debug.Assert(goal > 0, $"Set pie chart drawer to draw an income goal of {goal}. The income goal must always be positive and > 0.");

			if (this.income == income && this.goal == goal)
				return;

			if (pieChartElement == null)
				return;

			this.income = income;
			this.goal = goal;

			Update();
		}

		public void SetColors(
			Color incomeColor,
			Color goalColor,
			Color goalTextColor,
			Color negativeIncomeColor
		)
		{
			this.incomeColor = incomeColor;
			this.goalColor = goalColor;
			this.goalTextColor = goalTextColor;
			this.negativeIncomeColor = negativeIncomeColor;

			if (pieChartElement == null)
				return;

			Update();
		}

		private void Update()
		{
			pieChartIncomeLabel.text = FormatIncomeOrGoal(income);
			pieChartIncomeLabel.style.color = GetIncomeColor();
			pieChartGoalLabel.text = FormatIncomeOrGoal(goal);
			pieChartGoalLabel.style.color = GetGoalColor();

			// Invoke calling DrawPieChart via generateVisualContent.
			pieChartElement.MarkDirtyRepaint();
		}

		private static string FormatIncomeOrGoal(int incomeOrGoal)
		{
			return (incomeOrGoal >= 0 ? "+" : "-") + $"${Mathf.Abs(incomeOrGoal)}";
		}

		private Color GetIncomeColor()
		{
			return (income >= 0) ? incomeColor : negativeIncomeColor;
		}

		private Color GetGoalColor()
		{
			return (income < goal) ? goalTextColor : incomeColor;
		}

		private void DrawPieChart(MeshGenerationContext ctx)
		{
			// TODO: Handle case where goal == 0?

			Vector2 center = new Vector2(pieChartElement.resolvedStyle.width / 2, pieChartElement.resolvedStyle.height / 2);
			float radius = Mathf.Min(pieChartElement.resolvedStyle.width, pieChartElement.resolvedStyle.height) / 2;

			IncomePieChartDrawingUtils.DrawPieChart(
				ctx,
				income,
				goal,
				center,
				radius,
				incomeColor,
				goalColor
			);
		}
    }
}
