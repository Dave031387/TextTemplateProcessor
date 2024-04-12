namespace TextTemplateProcessor.TestShared
{
    using System;
    using System.Collections.Generic;

    public class MethodCallOrderVerifier
    {
        public record MethodCallOrder(MethodCall Call, int Order);
        public record MethodCallExpectedOrder(MethodCall FirstCall, MethodCall SecondCall);
        public List<MethodCallOrder> ActualOrderList { get; } = new();
        public int CallOrder { get; set; } = 0;
        public List<MethodCallExpectedOrder> ExpectedOrderList { get; } = new();
        public List<MethodCall> FirstTimeList { get; } = new();

        public void DefineExpectedCallOrder(MethodCall firstCall, MethodCall secondCall)
            => ExpectedOrderList.Add(new(firstCall, secondCall));

        public Action GetCallOrderAction(MethodCall methodCall, Action? callerAction = null, bool onFirstTimeOnly = false, Action? firstTimeAction = null)
        {
            return onFirstTimeOnly
                ? (() =>
                {
                    if (FirstTimeList.Contains(methodCall))
                    {
                        callerAction?.Invoke();
                    }
                    else
                    {
                        FirstTimeList.Add(methodCall);
                        ActualOrderList.Add(new(methodCall, ++CallOrder));
                        firstTimeAction?.Invoke();
                    }
                })
                : (() =>
                {
                    ActualOrderList.Add(new(methodCall, ++CallOrder));
                    callerAction?.Invoke();
                });
        }

        public void Reset()
        {
            ActualOrderList.Clear();
            ExpectedOrderList.Clear();
            FirstTimeList.Clear();
            CallOrder = 0;
        }

        public void Verify()
        {
            foreach (MethodCallExpectedOrder expectedOrder in ExpectedOrderList)
            {
                string firstCallName = Enum.GetName(typeof(MethodCall), expectedOrder.FirstCall)!;
                int firstCallOrder = 0;
                string secondCallName = Enum.GetName(typeof(MethodCall), expectedOrder.SecondCall)!;
                int secondCallOrder = 0;

                foreach (MethodCallOrder actualOrder in ActualOrderList)
                {
                    if (actualOrder.Call == expectedOrder.FirstCall)
                    {
                        firstCallOrder = actualOrder.Order;
                    }
                    else if (actualOrder.Call == expectedOrder.SecondCall)
                    {
                        secondCallOrder = actualOrder.Order;
                    }
                }

                firstCallOrder
                    .Should()
                    .BePositive($"the call order count for {firstCallName} should be greater than 0");
                secondCallOrder
                    .Should()
                    .BePositive($"the call order count for {secondCallName} should be greater than 0");
                secondCallOrder
                    .Should()
                    .BeGreaterThan(firstCallOrder, $"{secondCallName} should be called after {firstCallName}");
            }
        }
    }
}