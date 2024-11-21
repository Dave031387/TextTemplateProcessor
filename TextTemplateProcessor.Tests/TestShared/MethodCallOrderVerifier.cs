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
        private List<MethodCallOrder> ExpectedOrderList { get; } = [];

        /// <summary>
        /// Gets a dictionary whose key is a <see cref="MethodCallToken" /> and whose value gives
        /// the number of times that <see cref="MethodCallToken" /> has been called during the unit
        /// test.
        /// </summary>
        private Dictionary<MethodCallToken, int> MethodCallCounter { get; } = [];

        /// <summary>
        /// Gets a list of <see cref="MethodCall" /> objects. The objects will appear in the list in
        /// the order that each method was called during the unit test.
        /// </summary>
        private List<MethodCall> MethodCallList { get; } = [];

        /// <summary>
        /// Gets a list of <see cref="MethodCallToken" /> values for which at least one
        /// <see cref="MethodCallOrder" /> record specified a negative call number.
        /// </summary>
        private List<MethodCallToken> RelativeMethodCalls { get; } = [];

        /// <summary>
        /// This record defines the order of two method calls within a single unit test.
        /// </summary>
        /// <param name="FirstCall">
        /// The <see cref="MethodCall" /> that should be invoked first in sequence.
        /// </param>
        /// <param name="SecondCall">
        /// The <see cref="MethodCall" /> that should be invoked second in sequence.
        /// </param>
        private sealed record MethodCallOrder(MethodCall FirstCall, MethodCall SecondCall);

        /// <summary>
        /// This record represents a single method call during a unit test.
        /// </summary>
        /// <param name="MethodCallToken">
        /// The <see cref="MethodCallToken" /> that corresponds to the method being called in the
        /// unit test.
        /// </param>
        /// <param name="CallNumber">
        /// If the <paramref name="MethodCallToken" /> specified on this record is called more than
        /// once in a single unit test, then each occurrence can be assigned a unique
        /// <paramref name="CallNumber" /> to tell them apart.
        /// </param>
        private sealed record MethodCall(MethodCallToken MethodCallToken, int CallNumber = 0);

        /// <summary>
        /// Defines an expected method call order sequence to be verified later by calling the
        /// <see cref="Verify" /> method.
        /// </summary>
        /// <param name="firstCall">
        /// The <see cref="MethodCallToken" /> of the first method to be called in sequence.
        /// </param>
        /// <param name="secondCall">
        /// The <see cref="MethodCallToken" /> of the second method to be called in sequence.
        /// </param>
        /// <param name="firstCallNumber">
        /// Optional call number of the <paramref name="firstCall" /> method. (default is 0)
        /// </param>
        /// <param name="secondCallNumber">
        /// Optional call number of the <paramref name="secondCall" /> method. (default is 0)
        /// </param>
        /// <remarks>
        /// A negative value for either <paramref name="firstCallNumber" /> or
        /// <paramref name="secondCallNumber" /> indicates the specific method call number for the
        /// associated <paramref name="firstCall" /> or <paramref name="secondCall" /> methods. For
        /// example, -1 refers to the first time the method is called, -2 the second time the method
        /// is called, and so on.
        /// <para>
        /// Positive numbers, on the other hand, are used only to distinguish between instances of a
        /// method that is called multiple times with different parameter values. For example, if
        /// MethodA is called three times with different parameter values each time, then a unique
        /// positive number should be assigned to each call.
        /// </para>
        /// </remarks>
        public void DefineExpectedCallOrder(MethodCallToken firstCall, MethodCallToken secondCall, int firstCallNumber = 0, int secondCallNumber = 0)
        {
            MethodCall firstMethodCall = new(firstCall, firstCallNumber);
            MethodCall secondMethodCall = new(secondCall, secondCallNumber);
            ExpectedOrderList.Add(new(firstMethodCall, secondMethodCall));
        }

        /// <summary>
        /// This method returns an Action that can be assigned to the Callback method of a Moq
        /// Setup. The Action records the method call for future verification.
        /// </summary>
        /// <param name="methodCallToken">
        /// The <see cref="MethodCallToken" /> of the method being called.
        /// </param>
        /// <param name="callNumber">
        /// An optional call number used to distinguish the <paramref name="methodCallToken" /> if
        /// the associated method appears with different parameter values in two or more Moq Setups.
        /// </param>
        /// <returns>
        /// An Action that can be assigned to the Callback method of a Moq Setup.
        /// </returns>
        public Action GetCallOrderAction(MethodCallToken methodCallToken, int callNumber = 0)
        {
            return () =>
            {
                MethodCallCounter[methodCallToken] = MethodCallCounter.TryGetValue(methodCallToken, out int value) ? ++value : 1;
                MethodCallList.Add(new(methodCallToken, callNumber));

                if (callNumber < 0 && !RelativeMethodCalls.Contains(methodCallToken))
                {
                    RelativeMethodCalls.Add(methodCallToken);
                }
            };
        }

        /// <summary>
        /// This method resets the state of the <see cref="MethodCallOrderVerifier" /> class to its
        /// initial state.
        /// </summary>
        public void Reset()
        {
            MethodCallList.Clear();
            ExpectedOrderList.Clear();
            MethodCallCounter.Clear();
            RelativeMethodCalls.Clear();
        }

        /// <summary>
        /// This method iterates through the defined method call order objects and verifies that
        /// each method was called in the expected order.
        /// <para>
        /// Any discrepancies between the expected and actual method call order will result in an
        /// assertion failure with an appropriate message identifying the cause of the failure.
        /// </para>
        /// </summary>
        public void Verify()
        {
            foreach (MethodCallOrder expectedOrder in ExpectedOrderList)
            {
                MethodCallToken firstCallToken = expectedOrder.FirstCall.MethodCallToken;
                MethodCallToken secondCallToken = expectedOrder.SecondCall.MethodCallToken;
                string firstCallName = firstCallToken.Name;
                string secondCallName = secondCallToken.Name;
                int firstCallNumber = expectedOrder.FirstCall.CallNumber;
                int secondCallNumber = expectedOrder.SecondCall.CallNumber;
                int firstCallCounter = firstCallNumber;
                int secondCallCounter = secondCallNumber;
                string firstDisplayName = firstCallNumber is 0
                    ? firstCallName
                    : firstCallNumber < 0
                    ? $"{firstCallName}[+{-firstCallNumber}]"
                    : $"{firstCallName}[{firstCallNumber}]";
                string secondDisplayName = secondCallNumber is 0
                    ? secondCallName
                    : secondCallNumber < 0
                    ? $"{secondCallName}[+{-secondCallNumber}]"
                    : $"{secondCallName}[{secondCallNumber}]";
                int firstCallOrder = 0;
                int secondCallOrder = 0;

                expectedOrder.FirstCall
                    .Should()
                    .NotBe(expectedOrder.SecondCall, $"the first and second method calls must not both be {firstDisplayName}");

                if (RelativeMethodCalls.Contains(firstCallToken))
                {
                    firstCallNumber
                        .Should()
                        .BeNegative($"all instances of {firstCallName} must specify a negative call number if any other instances do");
                }

                if (RelativeMethodCalls.Contains(secondCallToken))
                {
                    secondCallNumber
                        .Should()
                        .BeNegative($"all instances of {secondCallName} must specify a negative call number if any other instances do");
                }

                if (firstCallToken == secondCallToken && firstCallNumber < 0)
                {
                    firstCallNumber
                        .Should()
                        .BeGreaterThan(secondCallNumber, $"{secondDisplayName} can't come before {firstDisplayName}");
                }

                for (int i = 0; i < MethodCallList.Count; i++)
                {
                    MethodCall methodCall = MethodCallList[i];
                    MethodCallToken methodCallToken = methodCall.MethodCallToken;
                    int methodCallNumber = methodCall.CallNumber;

                    if (firstCallOrder == 0 && methodCallToken == firstCallToken)
                    {
                        if (++firstCallCounter < 0)
                        {
                            continue;
                        }

                        if (firstCallNumber < 0 || methodCallNumber == firstCallNumber)
                        {
                            firstCallOrder = i + 1;
                        }
                    }
                    else if (methodCallToken == secondCallToken)
                    {
                        if (++secondCallCounter < 0)
                        {
                            continue;
                        }

                        if (secondCallNumber < 0 || methodCallNumber == secondCallNumber)
                        {
                            secondCallOrder = i + 1;

                            if (firstCallOrder > 0 || secondCallNumber < 0)
                            {
                                break;
                            }
                        }
                    }
                }

                firstCallOrder
                    .Should()
                    .BePositive($"the call sequence for {firstDisplayName} should be greater than 0");
                secondCallOrder
                    .Should()
                    .BePositive($"the call sequence for {secondDisplayName} should be greater than 0");
                secondCallOrder
                    .Should()
                    .BeGreaterThan(firstCallOrder, $"{secondDisplayName} should be called after {firstDisplayName}");
            }
        }
    }
}