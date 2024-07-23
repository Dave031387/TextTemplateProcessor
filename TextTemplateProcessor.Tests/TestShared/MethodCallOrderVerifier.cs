namespace TextTemplateProcessor.TestShared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class can be used to verify the order of method calls in a unit test. It was written to
    /// be used with the Moq mocking framework, but can be used with other frameworks that provide a
    /// callback functionality on their Setup method.
    /// </summary>
    public class MethodCallOrderVerifier
    {
        /// <summary>
        /// Gets a list of <see cref="MethodCallOrder" /> objects that define the expected order of
        /// method calls.
        /// </summary>
        public List<MethodCallOrder> ExpectedOrderList { get; } = [];

        /// <summary>
        /// Gets a dictionary whose key is a <see cref="MethodCallID" /> and whose value gives the
        /// number of times that <see cref="MethodCallID" /> has been called during the unit test.
        /// </summary>
        public Dictionary<MethodCallID, int> MethodCallCounter { get; } = [];

        /// <summary>
        /// Gets a list of <see cref="MethodCall" /> objects. The objects will appear in the list in
        /// the order that each method was called during the unit test.
        /// </summary>
        public List<MethodCall> MethodCallList { get; } = [];

        /// <summary>
        /// This record defines the order of two method calls within a single unit test.
        /// </summary>
        /// <param name="FirstCall">
        /// The <see cref="MethodCall" /> that should be invoked first in sequence.
        /// </param>
        /// <param name="SecondCall">
        /// The <see cref="MethodCall" /> that should be invoked second in sequence.
        /// </param>
        public record MethodCallOrder(MethodCall FirstCall, MethodCall SecondCall);

        /// <summary>
        /// This record represents a single method call during a unit test.
        /// </summary>
        /// <param name="MethodCallID">
        /// The <see cref="MethodCallID" /> that corresponds to the method being called in the unit
        /// test.
        /// </param>
        /// <param name="CallNumber">
        /// If the <paramref name="MethodCallID" /> specified on this record is called more than
        /// once in a single unit test, then each occurrence can be assigned a unique
        /// <paramref name="CallNumber" /> to tell them apart.
        /// </param>
        public record MethodCall(MethodCallID MethodCallID, int CallNumber = 0);

        /// <summary>
        /// This method adds a new <see cref="MethodCallOrder" /> object to the
        /// <see cref="ExpectedOrderList" />.
        /// </summary>
        /// <param name="firstCall">
        /// The <see cref="MethodCallID" /> of the first method to be called in sequence.
        /// </param>
        /// <param name="secondCall">
        /// The <see cref="MethodCallID" /> of the second method to be called in sequence.
        /// </param>
        /// <param name="firstCallNumber">
        /// Optional call number of the <paramref name="firstCall" /> method. (default is 0)
        /// </param>
        /// <param name="secondCallNumber">
        /// Optional call number of the <paramref name="secondCall" /> method. (default is 0)
        /// </param>
        public void DefineExpectedCallOrder(MethodCallID firstCall, MethodCallID secondCall, int firstCallNumber = 0, int secondCallNumber = 0)
        {
            MethodCall firstMethodCall = new(firstCall, firstCallNumber);
            MethodCall secondMethodCall = new(secondCall, secondCallNumber);
            ExpectedOrderList.Add(new(firstMethodCall, secondMethodCall));
        }

        /// <summary>
        /// This method returns an Action that can be assigned to the Callback method of a Moq
        /// Setup. The Action records the method call and optionally invokes other actions if
        /// specified.
        /// </summary>
        /// <param name="methodCallID">
        /// The <see cref="MethodCallID" /> of the method being called.
        /// </param>
        /// <param name="callNumber">
        /// An optional call number used to distinguish the <paramref name="methodCallID" /> if that
        /// <see cref="MethodCallID" /> is called more than once in a single execution of the unit
        /// test.
        /// </param>
        /// <param name="callerAction">
        /// An optional Action to invoke each time the associated Moq Setup is invoked. This Action
        /// will be skipped the first time if the <paramref name="firstTimeAction" /> is specified.
        /// </param>
        /// <param name="firstTimeAction">
        /// An optional Action to invoke the first time the associated Moq Setup is invoked. If both
        /// the <paramref name="firstTimeAction" /> and <paramref name="callerAction" /> are
        /// specified, then only the <paramref name="firstTimeAction" /> will get invoked the first
        /// time the Moq Setup is invoked.
        /// </param>
        /// <returns>
        /// An Action that can be assigned to the Callback method of a Moq Setup.
        /// </returns>
        public Action GetCallOrderAction(MethodCallID methodCallID, int callNumber = 0, Action? callerAction = null, Action? firstTimeAction = null)
        {
            return () =>
            {
                if (MethodCallCounter.TryGetValue(methodCallID, out int value))
                {
                    MethodCallCounter[methodCallID] = ++value;
                    callerAction?.Invoke();
                }
                else
                {
                    MethodCallCounter[methodCallID] = 1;

                    if (firstTimeAction is null)
                    {
                        callerAction?.Invoke();
                    }
                    else
                    {
                        firstTimeAction.Invoke();
                    }
                }

                MethodCallList.Add(new(methodCallID, callNumber));
            };
        }

        /// <summary>
        /// This method resets the state of the <see cref="MethodCallOrderVerifier" /> class by
        /// clearing the contents of the <see cref="MethodCallList" />,
        /// <see cref="MethodCallCounter" />, and <see cref="ExpectedOrderList" /> properties.
        /// </summary>
        public void Reset()
        {
            MethodCallList.Clear();
            ExpectedOrderList.Clear();
            MethodCallCounter.Clear();
        }

        /// <summary>
        /// This method iterates through the <see cref="MethodCallOrder" /> objects in the
        /// <see cref="ExpectedOrderList" /> property and compares each entry with the actual method
        /// call order that is stored in the <see cref="MethodCallList" /> property.
        /// <para>
        /// Any discrepancies between the expected and actual method call order will result in an
        /// assertion failure with an appropriate message identifying the cause of the failure.
        /// </para>
        /// </summary>
        public void Verify()
        {
            foreach (MethodCallOrder expectedOrder in ExpectedOrderList)
            {
                string firstCallID = Enum.GetName(typeof(MethodCallID), expectedOrder.FirstCall.MethodCallID)!;
                string secondCallID = Enum.GetName(typeof(MethodCallID), expectedOrder.SecondCall.MethodCallID)!;
                int firstCallNumber = expectedOrder.FirstCall.CallNumber;
                int secondCallNumber = expectedOrder.SecondCall.CallNumber;
                string firstCallName = firstCallNumber is 0 ? firstCallID : $"{firstCallID}[{firstCallNumber}]";
                string secondCallName = secondCallNumber is 0 ? secondCallID : $"{secondCallID}[{secondCallNumber}]";
                int firstCallOrder = 0;
                int secondCallOrder = 0;

                for (int i = 0; i < MethodCallList.Count; i++)
                {
                    MethodCall methodCall = MethodCallList[i];

                    if (methodCall == expectedOrder.FirstCall)
                    {
                        firstCallOrder = i + 1;
                    }
                    else if (methodCall == expectedOrder.SecondCall)
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