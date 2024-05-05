namespace TextTemplateProcessor.TestShared
{
    using System;
    using System.Collections.Generic;

    public class MethodCallOrderVerifier
    {
        public record MethodCallExpectedOrder(MethodCall FirstCall, MethodCall SecondCall);
        public List<MethodCall> CallOrderList { get; } = new();
        public List<MethodCallExpectedOrder> ExpectedOrderList { get; } = new();

        public void DefineExpectedCallOrder(MethodCall firstCall, MethodCall secondCall)
            => ExpectedOrderList.Add(new(firstCall, secondCall));

        public Action GetCallOrderAction(MethodCall methodCall, Action? callerAction = null, Action? firstTimeAction = null)
        {
            return () =>
                {
                    if (CallOrderList.Contains(methodCall) || firstTimeAction is null)
                    {
                        callerAction?.Invoke();
                    }
                    else
                    {
                        firstTimeAction?.Invoke();
                    }

                    CallOrderList.Add(methodCall);
                };
        }

        public void Reset()
        {
            CallOrderList.Clear();
            ExpectedOrderList.Clear();
        }

        public void Verify()
        {
            foreach (MethodCallExpectedOrder expectedOrder in ExpectedOrderList)
            {
                string firstCallName = Enum.GetName(typeof(MethodCall), expectedOrder.FirstCall)!;
                int firstCallOrder = 0;
                string secondCallName = Enum.GetName(typeof(MethodCall), expectedOrder.SecondCall)!;
                int secondCallOrder = 0;

                for (int i = 0; i < CallOrderList.Count; i++)
                {
                    if (CallOrderList[i] == expectedOrder.FirstCall)
                    {
                        firstCallOrder = i + 1;
                    }
                    else if (CallOrderList[i] == expectedOrder.SecondCall)
                    {
                        secondCallOrder = i + 1;

                        if (firstCallOrder > 0)
                        {
                            break;
                        }
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