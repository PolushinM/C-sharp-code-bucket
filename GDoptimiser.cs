public class GDSupervisor {
 
  // TODO Сделать что-нибудь с этой функцией TryParameters!!!
  private decimal TryParameters(Dictionary<string, decimal> parameters) 
    => ScriptDelegate_(parameters);
 
  Func<Dictionary<string, decimal>, decimal> ScriptDelegate_;
 
  public void SetScriptDelegate(Func<Dictionary<string, decimal>, decimal> ScriptDelegate) 
    => ScriptDelegate_ = ScriptDelegate;
 
  GDSupervisor(Func<Dictionary<string, decimal>, decimal> ScriptDelegate, 
              Dictionary<string, (decimal minValue, decimal maxValue, decimal step)> restrictions, 
              Dictionary<string, decimal> initialParameters) {
    ScriptDelegate_ = ScriptDelegate;
    restrictions_ = restrictions;
    initialParameters_ = initialParameters;
  }
 
  GDSupervisor(Func<Dictionary<string, decimal>, decimal> ScriptDelegate, 
              Dictionary<string, (decimal minValue, decimal maxValue, decimal step)> restrictions) {
    ScriptDelegate_ = ScriptDelegate;
    restrictions_ = restrictions;
    initialParameters_ = InitAvgParameters(restrictions);
  }
 
  Dictionary<string, (decimal minValue, decimal maxValue, decimal step)> restrictions_;
  Dictionary<string, decimal> initialParameters_;
 
  private Dictionary<string, decimal> InitAvgParameters(Dictionary<string, 
                                        (decimal minValue, decimal maxValue, decimal step)> restrictions) {
    Dictionary<string, decimal> result = new Dictionary<string, decimal>(restrictions.Count);
    foreach (var restr in restrictions) {
      result.Add(restr.Key, restr.Value.minValue * 0.5m + restr.Value.maxValue * 0.5m);
    }
    return result;
  }
 
  public void SetInitParameters(Dictionary<string, decimal> initialParameters) {
    initialParameters_ = initialParameters;
  }
 
  private static Dictionary<string, decimal> CalculateSteps(Dictionary<string, 
                                                     (decimal minValue, decimal maxValue, decimal initStep)> restrictions_, int step = 0) {
    Dictionary<string, decimal> steps = new Dictionary<string, decimal>();
    foreach (var restr in restrictions_) {
      decimal modifier = (decimal)(1.0 / Math.Sqrt(1 + step));
      steps.Add(restr.Key, restr.Value.initStep / modifier);
    }
    return steps;
  }
 
  private static Dictionary<string, decimal> CalculateParameters(Dictionary<string, decimal> parameters,
                                                  Dictionary<string, (decimal minValue, decimal maxValue, decimal initStep)> restrictions,
                                                  int step) {
    Dictionary<string, decimal> newParameters = new Dictionary<string, decimal>(parameters.Count);
    Dictionary<string, decimal> steps = CalculateSteps(restrictions, step);
    // Calculate new parameters within borders
    foreach (var param in parameters) {
      decimal value = param.Value + steps[param.Key];
      if (value > restrictions[param.Key].maxValue)
        value = restrictions[param.Key].maxValue;
      if (value < restrictions[param.Key].minValue)
        value = restrictions[param.Key].minValue;
      newParameters.Add(param.Key, value);
    }
    // Check delta > deltamin
    decimal deltaMult = 0.1m;
    foreach (var param in newParameters) {
      if (Math.Abs(param.Value - parameters[param.Key]) < steps[param.Key] * deltaMult) {
        steps[param.Key] = -steps[param.Key];
        newParameters[param.Key] += steps[param.Key];
      }
    }
    return newParameters;
  }
 
  public void SetRestrictions(Dictionary<string, 
                              (decimal minValue, decimal maxValue, decimal initStep)> restrictions) => restrictions_ = restrictions;
 
  private Dictionary<string, decimal> Gradient(Dictionary<string, decimal> current,
                                                Dictionary<string, (decimal minValue, decimal maxValue, decimal step)> restrictions,
                                                int step) {
    Dictionary<string, decimal> parameters = new Dictionary<string, decimal>(current.Count);
    Dictionary<string, decimal> grad = new Dictionary<string, decimal>(current.Count);
    decimal OMValue = TryParameters(current);
    parameters = CalculateParameters(current, restrictions, step);
    foreach (var param in current) {
      decimal derivative = (TryParameters(parameters) - OMValue) / (parameters[param.Key] - param.Value);
      grad.Add(param.Key, derivative);
    }
    return grad;
  }
 
  private static decimal Distance(Dictionary<string, decimal> first, 
                                  Dictionary<string, decimal> second, 
                                  Dictionary<string, (decimal minValue, decimal maxValue, decimal step)> restrictions) {
    decimal sum = 0;
    foreach (var item in first) {
      decimal value = (item.Value - second[item.Key]) / (restrictions[item.Key].maxValue - restrictions[item.Key].minValue);
      sum += value * value;
    }
    return (decimal)Math.Sqrt((double)sum);
  }
 
  private (Dictionary<string, decimal> w_next, decimal weight_evolution, Dictionary<string, decimal> grad)
    EvalWNext(decimal eta, Dictionary<string, decimal> w_current, int step) {
    // Calculate gradient
    Dictionary<string, decimal> grad = Gradient(w_current, restrictions_, step);
    // Make step of gradient descent
    Dictionary<string, decimal> w_next = new Dictionary<string, decimal>(w_current.Count);
    foreach (var param in w_current)
      w_next.Add(param.Key, (param.Value + eta * grad[param.Key]));
    decimal weight_evolution = Distance(w_current, w_next, restrictions_);
    return (w_next, weight_evolution, grad);
  }
 
  Func<int, Dictionary<string, decimal>, decimal, bool> StatusDelegate_;
 
  public void SetStatusDelegate(Func<int, Dictionary<string, decimal>, decimal, bool> StatusDelegate) {
    StatusDelegate_ = StatusDelegate;
  }
 
  private bool ReturnStatus(int step, Dictionary<string, decimal> currentParameters, decimal evolution)
    => StatusDelegate_(step, currentParameters, evolution);
 
  public Dictionary<string, decimal> GradientDescent(decimal eta = 0.01m, decimal epsilon = 1m, int maxSteps = 10) {
    // Initialise parameters 
    Dictionary<string, decimal> parameters = initialParameters_;
    Dictionary<string, decimal> parameters_next;
    decimal evolution;
    Dictionary<string, decimal> grad;
    int step = 0;
    (parameters_next, evolution, grad) = EvalWNext(eta, parameters, step);
    // Repeat until the weights vector converges
    while ((evolution > epsilon) && (maxSteps - step > 0)) {
      (parameters_next, evolution, grad) = EvalWNext(eta, parameters, step);
      step++;
      ReturnStatus(step, parameters, evolution);
    }
    return parameters;
  }
}