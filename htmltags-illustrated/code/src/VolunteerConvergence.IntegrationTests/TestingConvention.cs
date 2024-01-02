using Fixie;

namespace VolunteerConvergence.IntegrationTests
{
    public class TestingConvention : Discovery, Execution
    {
        public TestingConvention()
        {
            // In a class, any public method that does not have the name "SetUp" or "TearDown" is considered a test
            Methods.Where(m => m.Name != "SetUp" && m.Name != "TearDown");
        }

        // What do do when running a test (case).
        // This is coded to:
        //      1) instantiate a new instance of the test class for each test ('instance per test')
        //      2) run the method named SetUp, if it exists
        //      3) run the method (the test)
        //      4) run the method named TearDown, if it exists
        public void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                SetUp(instance);

                @case.Execute(instance);

                TearDown(instance);
            });
        }

        static void SetUp(object instance)
        {
            instance.GetType().GetMethod("SetUp")?.Execute(instance);
        }

        static void TearDown(object instance)
        {
            instance.GetType().GetMethod("TearDown")?.Execute(instance);
        }
    }
}