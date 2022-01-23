public static List<decimal> Differentiate(IList<decimal> profit) {
  int size = profit.Count;
  List<decimal> result = new List<decimal>(size) { 0 };
  for (int i = 1; i < size; ++i)
	result.Add(profit[i] - profit[i - 1]);
  return result;
}

public static decimal StandardDeviation(List<decimal> data) {
  decimal average = data.Sum() / data.Count;
  decimal dispersion = data.Sum(value => (value - average) * (value - average)) / data.Count;
  return (decimal)Math.Sqrt((double)dispersion);
}

public static decimal ExcessKurtosis(List<decimal> data) {
  int size = data.Count;
  decimal fourthMoment = 0;
  decimal dispersion = 0;
  decimal average = data.Sum() / size;
  foreach (decimal value in data) {
	decimal dev = (value - average);
	fourthMoment += dev * dev * dev * dev / size;
	dispersion += dev * dev / size;
  }
  return fourthMoment / (dispersion * dispersion) - 3;
}

public static decimal Skewness(List<decimal> data) {
  int size = data.Count;
  decimal thirdMoment = 0;
  decimal dispersion = 0;
  decimal average = data.Sum() / size;
  foreach (decimal value in data) {
	decimal dev = (value - average);
	thirdMoment += dev * dev * dev / size;
	dispersion += dev * dev / size;
  }
  return thirdMoment / (decimal)Math.Pow((double)dispersion, 1.5);
}

public static List<(decimal value, decimal density)> Distribution(IList<decimal> data, int intervals) {
  List<(decimal value, decimal density)> result = new List<(decimal value, decimal density)>(intervals);
  List<decimal> densityList = new List<decimal>(intervals);
  List<decimal> borders = new List<decimal>(intervals);
  decimal step = (data.Max() - data.Min()) / intervals;

  for (decimal border = data.Min() + step; border <= data.Max(); border += step) {
	borders.Add(border);
	densityList.Add(0m);
  }

  foreach (decimal item in data)
	for (int i = 0; i < intervals; ++i)
	  if (item <= borders[i]) {
		densityList[i]++;
		break;
	  }

  for (int i = 0; i < intervals; ++i)
	result.Add((borders[i] - step / 2, densityList[i] * intervals / data.Count()));

  return result;
}

public static decimal ShiftedСorrelation(IList<decimal> indicator, IList<decimal> price, ushort offset) {
  int count = price.Count;
  if (count != indicator.Count || count < 2 + offset)
	return 0.0m;
  // Calculate averages.
  decimal priceAverage = 0;
  decimal indicatorAverage = 0;
  for (int i = 0; i < count - offset; ++i) {
	priceAverage += price[i + offset];
	indicatorAverage += indicator[i];
  }
  priceAverage /= (count - offset);
  indicatorAverage /= (count - offset);
  // Calculate dispersion and covariation.
  decimal covariation = 0;
  decimal dispersionPrice = 0;
  decimal dispersionIndicator = 0;
  decimal l, r;
  for (int i = 0; i < count - offset; ++i) {
	l = price[i + offset] - priceAverage;
	r = indicator[i] - indicatorAverage;
	covariation += l * r;
	dispersionPrice += l * l;
	dispersionIndicator += r * r;
  }
  return covariation / (decimal)Math.Sqrt(((double)dispersionPrice * (double)dispersionIndicator));
}

public static decimal SmoothedСorrelation(IList<decimal> indicator, IList<decimal> price, ushort offset) {
  int count = price.Count;
  if (count != indicator.Count || count < 2 + offset)
	return 0.0m;
  // Calculate averages.
  decimal priceAverage = 0;
  decimal indicatorAverage = 0;
  for (int i = 0; i < count - offset; ++i) {
	priceAverage += price[i + offset / 2];
	indicatorAverage += indicator[i];
  }
  priceAverage /= (count - offset);
  indicatorAverage /= (count - offset);
  // Calculate dispersion and covariation.
  decimal covariation = 0;
  decimal dispersionPrice = 0;
  decimal dispersionIndicator = 0;
  decimal l, r;
  for (int i = 0; i < count - offset; ++i) {
	l = 0;
	for (int j = 1; j <= offset; ++j)
	  l += price[i + j];

	l = l / offset - priceAverage;
	r = indicator[i] - indicatorAverage;
	covariation += l * r;
	dispersionPrice += l * l;
	dispersionIndicator += r * r;
  }
  return covariation / (decimal)Math.Sqrt(((double)dispersionPrice * (double)dispersionIndicator));
}

public static decimal SimpleSharp(IList<decimal> profit, int period = 0) {
  if (profit.Count < 100 || period < 0)
	return 0.0m;
  if (period == 0)
	period = (int)Math.Sqrt(profit.Count * 20.0);
  int sharpSize = (profit.Count - 1) / period;
  int tail = (profit.Count - 1) % period;
  List<decimal> sharpList = new List<decimal>(sharpSize);
  for (int i = 0; i < sharpSize; ++i)
	sharpList.Add(profit[i * period + tail + period] - profit[i * period + tail]);
  decimal average = sharpList.Sum() / sharpList.Count;
  decimal dispersion = sharpList.Sum(value => (value - average) * (value - average)) / sharpList.Count;
  return average / (decimal)Math.Sqrt((double)dispersion);
}

public static decimal WeightedProfit(IList<decimal> profit) {
  int count = profit.Count;
  if (count < 2)
	return 0.0m;
  List<decimal> difProfit = Differentiate(profit);
  decimal sum = 0;
  for (int i = 1; i < count; ++i)
	sum += difProfit[i] * i;
  return sum / (count - 1) * 2;
}

public static decimal Sharp(IList<decimal> profit, decimal deposit, DateTime beginDate,
							DateTime endDate, double riskFreeRate = 0.07, int period = 0) {
  // Input validation.
  if (profit.Count < 100 || period < 0)
	return 0.0m;
  // Calculate default period.
  if (period == 0)
	period = (int)Math.Sqrt(profit.Count * 20.0);
  // Calculate duration of whole period of time in years.
  TimeSpan span = endDate - beginDate;
  decimal years = (decimal)span.TotalSeconds / 31557600.0m;
  // Filling of profit-in-periods array.
  int sharpSize = (profit.Count - 1) / period;
  // Sharp size validation.
  if (sharpSize < 2)
	return 0.0m;
  int tail = (profit.Count - 1) % period;
  List<decimal> sharpList = new List<decimal>(sharpSize);
  // Risk free rate, referred to one period in array.
  decimal averageRFR = deposit * ((decimal)Math.Pow(riskFreeRate + 1.0, (double)years / sharpSize) - 1.0m);
  for (int i = 0; i < sharpSize; ++i)
	sharpList.Add(profit[i * period + tail + period] - profit[i * period + tail] - averageRFR);
  // Calculate Sharp coefficient.
  decimal average = sharpList.Sum() / sharpSize;
  decimal dispersion = sharpList.Sum(value => (value - average) * (value - average)) * years;
  return average / (decimal)Math.Sqrt((double)dispersion) * ((sharpSize - 1) / (years * 12 - 1));
}

