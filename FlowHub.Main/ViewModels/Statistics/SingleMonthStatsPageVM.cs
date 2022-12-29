﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace FlowHub.Main.ViewModels.Statistics;

[QueryProperty(nameof(SpecificMonthNumber), "MonthNumber")]
[QueryProperty(nameof(SpecificYearNumber), "YearNumber")]
[QueryProperty(nameof(PageTitle), nameof(PageTitle))]
[QueryProperty(nameof(ListOfExpenditures), "ExpendituresList")]
public partial class SingleMonthStatsPageVM :ObservableObject
{
    private readonly IExpendituresRepository expendituresService;

	public SingleMonthStatsPageVM(IExpendituresRepository expRepo)
	{
		expendituresService = expRepo;
	}

	[ObservableProperty]
	public int specificMonthNumber;

	[ObservableProperty]
	public int specificYearNumber;

    [ObservableProperty]
    public string pageTitle;

	[ObservableProperty]
	public int totalFlowOuts;

	[ObservableProperty]
	public double totalAmount;

	[ObservableProperty]
	public double averageAmount;

	[ObservableProperty]
	public double biggestAmount;

	[ObservableProperty]
	public string currency;

	[ObservableProperty]
	public ExpendituresModel singleExpenditure = new();

	[ObservableProperty]
	public ObservableCollection<ExpendituresModel> listOfExpenditures;
	public List<ISeries> PieSeries { get; set; }	

	[RelayCommand]
	public void PageLoaded()
	{

		TotalFlowOuts = ListOfExpenditures.Count;
		TotalAmount = ListOfExpenditures.Sum(exp => exp.AmountSpent);
		AverageAmount = TotalAmount / TotalFlowOuts;
		BiggestAmount = ListOfExpenditures.Max(exp => exp.AmountSpent);
		SingleExpenditure = ListOfExpenditures.MaxBy(exp => exp.AmountSpent);
		Currency = SingleExpenditure.Currency;

		PieSeries = new List<ISeries>();

		List<PieSeries<ExpendituresModel>> ListOfPieSeries = new();
		foreach (var Exp in ListOfExpenditures)
		{
			PieSeries.
			Add(new PieSeries<double>
			{
				Values = new double[] { Exp.AmountSpent },
				Name = Exp.Reason,
				TooltipLabelFormatter =
				(ChartPoint) => $"{Exp.Reason}",
				Mapping = (exp, point) =>
				{
					point.PrimaryValue = Exp.AmountSpent;
					point.TertiaryValue = ListOfExpenditures.IndexOf(Exp);
				}
			});
		}

    }

	public void ChangeSelectedExp(int SelectedExpIndex)
	{
        SingleExpenditure = ListOfExpenditures[SelectedExpIndex];
    }
}
