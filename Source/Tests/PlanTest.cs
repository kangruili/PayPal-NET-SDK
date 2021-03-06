﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PayPal.Api;
using PayPal;
using System;

namespace PayPal.Testing
{
    /// <summary>
    /// Summary description for PlanTest
    /// </summary>
    [TestClass]
    public class PlanTest
    {
        public static readonly string PlanJson = 
            "{\"name\":\"T-Shirt of the Month Club Plan\"," +
            "\"description\":\"Template creation.\"," +
            "\"type\":\"FIXED\"," +
            "\"payment_definitions\":[" + PaymentDefinitionTest.PaymentDefinitionJson + "]," +
            "\"merchant_preferences\":" + MerchantPreferencesTest.MerchantPreferencesJson + "}";

        public static Plan GetPlan()
        {
            return JsonFormatter.ConvertFromJson<Plan>(PlanJson);
        }

        [TestMethod, TestCategory("Unit")]
        public void PlanObjectTest()
        {
            var testObject = GetPlan();
            Assert.AreEqual("T-Shirt of the Month Club Plan", testObject.name);
            Assert.AreEqual("Template creation.", testObject.description);
            Assert.AreEqual("FIXED", testObject.type);
            Assert.IsNotNull(testObject.payment_definitions);
            Assert.IsTrue(testObject.payment_definitions.Count == 1);
            Assert.IsNotNull(testObject.merchant_preferences);
        }

        [TestMethod, TestCategory("Unit")]
        public void PlanConvertToJsonTest()
        {
            Assert.IsFalse(GetPlan().ConvertToJson().Length == 0);
        }

        [TestMethod, TestCategory("Unit")]
        public void PlanToStringTest()
        {
            Assert.IsFalse(GetPlan().ToString().Length == 0);
        }

        [TestMethod, TestCategory("Functional")]
        public void PlanCreateAndGetTest()
        {
            try
            {
                var apiContext = TestingUtil.GetApiContext();
                var plan = GetPlan();
                var createdPlan = plan.Create(apiContext);
                Assert.IsTrue(!string.IsNullOrEmpty(createdPlan.id));
                Assert.AreEqual(plan.name, createdPlan.name);

                var retrievedPlan = Plan.Get(apiContext, createdPlan.id);
                Assert.IsNotNull(retrievedPlan);
                Assert.AreEqual(createdPlan.id, retrievedPlan.id);
                Assert.AreEqual("T-Shirt of the Month Club Plan", retrievedPlan.name);
                Assert.AreEqual("Template creation.", retrievedPlan.description);
                Assert.AreEqual("FIXED", retrievedPlan.type);
            }
            finally
            {
                TestingUtil.RecordConnectionDetails();
            }
        }

        [TestMethod, TestCategory("Functional")]
        public void PlanUpdateTest()
        {
            try
            {
                var apiContext = TestingUtil.GetApiContext();

                // Get a test plan for updating purposes.
                var plan = GetPlan();
                var createdPlan = plan.Create(TestingUtil.GetApiContext());
                var planId = createdPlan.id;

                // Create the patch request and update the description to a random value.
                var updatedDescription = Guid.NewGuid().ToString();
                var patch = new Patch();
                patch.op = "replace";
                patch.path = "/";
                patch.value = new Plan() { description = updatedDescription };
                var patchRequest = new PatchRequest();
                patchRequest.Add(patch);

                // Update the plan.
                createdPlan.Update(apiContext, patchRequest);

                // Verify the plan was updated successfully.
                var updatedPlan = Plan.Get(apiContext, planId);
                Assert.AreEqual(planId, updatedPlan.id);
                Assert.AreEqual(updatedDescription, updatedPlan.description);
            }
            finally
            {
                TestingUtil.RecordConnectionDetails();
            }
        }

        [TestMethod, TestCategory("Functional")]
        public void PlanListTest()
        {
            try
            {
                var planList = Plan.List(TestingUtil.GetApiContext());
                Assert.IsNotNull(planList);
                Assert.IsNotNull(planList.plans);
                Assert.IsTrue(planList.plans.Count > 0);
            }
            finally
            {
                TestingUtil.RecordConnectionDetails();
            }
        }

        [TestMethod, TestCategory("Functional")]
        public void PlanDeleteTest()
        {
            try
            {
                var plan = GetPlan();
                var createdPlan = plan.Create(TestingUtil.GetApiContext());
                var planId = createdPlan.id;

                // Create a patch request that will delete the plan
                var patchRequest = new PatchRequest
                {
                    new Patch
                    {
                        op = "replace",
                        path = "/",
                        value = new Plan
                        {
                            state = "DELETED"
                        }
                    }
                };

                createdPlan.Update(TestingUtil.GetApiContext(), patchRequest);

                // Attempting to retrieve the plan should result in a PayPalException being thrown.
                TestingUtil.AssertThrownException<PaymentsException>(() => Plan.Get(TestingUtil.GetApiContext(), planId));
            }
            finally
            {
                TestingUtil.RecordConnectionDetails();
            }
        }
    }
}
