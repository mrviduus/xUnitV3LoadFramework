#pragma warning disable IDE1006
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadFramework.Extensions.Framework;
using System;

[assembly: TestFramework(typeof(LoadTestFramework))]
[assembly: CaptureConsole]

namespace xUnitV3LoadTests;

//==================================//
// EXAMPLE 1: STANDARD WORKFLOW     //
//==================================// 

public class When_running_standard_load_scenarios : Specification
{
    protected override void EstablishContext() =>
        Console.WriteLine(">> EstablishContext called");

    protected override void Because() =>
        Console.WriteLine(">> Because called");

    [Load(order: 1, concurrency: 2, duration: 5000, interval: 500)]
    public void should_run_first_scenario() =>
        Console.WriteLine(">> Executing first scenario");

    [Load(order: 2, concurrency: 3, duration: 7000, interval: 300)]
    public void should_run_second_scenario() =>
        Console.WriteLine(">> Executing second scenario");

    [Load(order: 3, concurrency: 1, duration: 3000, interval: 1000, Skip = "testing skip")]
    public void should_skip_scenario() =>
        Console.WriteLine(">> Executing third scenario");
    
    
}

//=========================================================//
// EXAMPLE 2: VERIFYING LIFECYCLE HOOK EXECUTION ORDER     //
//=========================================================//

public class When_testing_all_lifecycle_hooks : Specification
{
    protected override void EstablishContext() =>
        Console.WriteLine(">> [Lifecycle] EstablishContext invoked");

    protected override void Because() =>
        Console.WriteLine(">> [Lifecycle] Because invoked");

    protected override void DestroyContext() =>
        Console.WriteLine(">> [Lifecycle] DestroyContext invoked");

    [Load(order: 1, concurrency: 1, duration: 2000, interval: 1000)]
    public void should_run_and_log_full_lifecycle() =>
        Console.WriteLine(">> Running lifecycle test");
}

//=========================================================//
// EXAMPLE 3: EXCEPTION DURING CONSTRUCTOR                 //
//=========================================================//

public class When_constructor_throws_exception : Specification
{
    public When_constructor_throws_exception() =>
        throw new Exception(">> Constructor exception");

    [Load(order: 1, concurrency: 1, duration: 1000, interval: 1000)]
    public void should_fail_due_to_constructor_error() { }
}

//=========================================================//
// EXAMPLE 4: EXCEPTION IN ESTABLISHCONTEXT                //
//=========================================================//

public class When_establish_context_throws : Specification
{
    protected override void EstablishContext() =>
        throw new Exception(">> EstablishContext failure");

    [Load(order: 1, concurrency: 1, duration: 1000, interval: 1000)]
    public void should_fail_due_to_context_setup() { }
}

//=========================================================//
// EXAMPLE 5: EXCEPTION IN BECAUSE                         //
//=========================================================//

public class When_because_throws : Specification
{
    protected override void Because() =>
        throw new Exception(">> Because failure");

    [Load(order: 1, concurrency: 1, duration: 1000, interval: 1000)]
    public void should_fail_due_to_action_error() { }
}

//=========================================================//
// EXAMPLE 6: EXCEPTION IN DESTROYCONTEXT                  //
//=========================================================//

public class When_destroy_context_throws : Specification
{
    protected override void DestroyContext() =>
        throw new Exception(">> DestroyContext failure");

    [Load(order: 1, concurrency: 1, duration: 1000, interval: 1000)]
    public void should_flag_cleanup_failure() { }
}