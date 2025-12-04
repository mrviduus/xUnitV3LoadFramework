# xUnitV3LoadFramework 

**Make your app super strong! 🚀**

Imagine your app is like a playground, and this tool helps you see what happens when LOTS of kids want to play at the same time! It's like having a playground test to make sure the swings don't break when everyone uses them together.

[![NuGet](https://img.shields.io/nuget/v/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![Downloads](https://img.shields.io/nuget/dt/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![xUnit v3](https://img.shields.io/badge/xUnit-v3.0-blue)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## What can it do? 🎈

* **Easy to use** - Just add one special sticker to your test and BOOM! It becomes a super test! 
* **Many friends at once** - Like having 100 kids play on the same slide at the same time
* **Shows you fun numbers** - Tells you how fast your app is and if anything breaks
* **Works with your tests** - Uses the same tests you already know how to write
* **Smart stopping** - Knows when to stop the game nicely or quickly
* **Patient waiting** - Waits for slow friends to finish what they're doing

## Quick Start - Let's Play! 🎮

### 1. Get the toy
```bash
dotnet add package xUnitV3LoadFramework
```

### 2. Make a super test

Think of this like planning a birthday party - you need to decide how many friends to invite and how long the party lasts!

#### Method 1: Using a Magic Sticker ✨
```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

public class MyPartyTests
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(concurrency: 5, duration: 3000, interval: 500)] // 5 friends for 3 seconds!
    public async Task Test_My_Website_Party()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Like asking "Can I have a cookie?" 5 times at once!
            var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode; // Did I get my cookie?
        });
        
        Assert.True(result.Success > 0, "At least someone should get a cookie!");
        Console.WriteLine($"🍪 {result.Success} kids got cookies!");
    }
}
```

#### Method 2: Building with Blocks 🧱
```csharp
using xUnitV3LoadFramework.Extensions;

public class MyBlockTests
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Fact]
    public async Task Test_Website_With_Building_Blocks()
    {
        var result = await LoadTestRunner.Create()
            .WithName("Cookie_Party") // Name your party!
            .WithConcurrency(10)      // 10 friends
            .WithDuration(TimeSpan.FromSeconds(5)) // Party for 5 seconds
            .WithInterval(TimeSpan.FromMilliseconds(200)) // New friend every 0.2 seconds
            .RunAsync(async () =>
            {
                // Everyone asks for cookies at the same time!
                var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
                response.EnsureSuccessStatusCode();
                // If no error = got the cookie! 🍪
            });

        Assert.True(result.Success > 0, "Someone should get cookies!");
        Console.WriteLine($"🎉 Party results: {result.Success} happy kids out of {result.Total}!");
        Console.WriteLine($"⚡ Speed: {result.RequestsPerSecond:F1} cookies per second!");
    }
}
```

### 3. See the party results! 🎊
```
🎉 Party Results:
   Total: 50
   Success: 48 🍪
   Failure: 2 😢
   RequestsPerSecond: 16
   Time: 3.2 seconds
```

## 🤔 Which way should I play?

**Use Method 1 (Magic Sticker)** when:
- You want the computer to find your party tests automatically
- You like putting stickers on things to make them special

**Use Method 2 (Building Blocks)** when:
- You want to decide exactly when the party happens
- You like building things step by step
- You want both regular tests AND party tests in the same place

### 🎈 More Fun Examples

#### Having Both Regular and Party Tests
```csharp
public class MixedFunTests
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Fact] // Regular test - just one kid
    public async Task Check_If_Cookie_Store_Works()
    {
        var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
        
        Assert.True(response.IsSuccessStatusCode, "The cookie store should be open!");
        Console.WriteLine("✅ Cookie store is working!");
    }

    [Fact] // Party test - LOTS of kids!
    public async Task Cookie_Store_Party_Test()
    {
        var result = await LoadTestRunner.Create()
            .WithConcurrency(8) // 8 kids want cookies
            .WithDuration(TimeSpan.FromSeconds(10)) // 10-second cookie rush!
            .WithInterval(TimeSpan.FromMilliseconds(300)) // New kid every 0.3 seconds
            .RunAsync(async () =>
            {
                var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
                response.EnsureSuccessStatusCode();
            });

        Assert.True(result.Success > result.Total * 0.9, "90% of kids should get cookies!");
        Console.WriteLine($"🍪 Cookie party: {result.Success} happy kids!");
    }
}
```

## 🎪 Super Cool Party Settings Guide

Think of party settings like planning the BEST birthday party ever!

### 🎯 What Each Setting Means

- **Concurrency** 👫: How many friends come to your party at the same time (like 5 or 10 kids)
- **Duration** ⏰: How long your party lasts (like 30 seconds or 5 minutes)
- **Interval** 🚪: How often new friends arrive (every second? every 2 seconds?)
- **TerminationMode** 🛑: How you end the party (nicely or quickly)
- **GracefulStopTimeout** ⏳: How long you wait for slow friends to finish their games

### 🎮 Easy Examples

#### Cookie Store Test (Simple)
```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.Extensions;

[Fact]
public async Task Test_Cookie_Store_Simple()
{
    using var httpClient = new HttpClient();
    
    var result = await LoadTestRunner.Create()
        .WithName("Cookie_Store_Test")
        .WithConcurrency(5)           // 5 kids want cookies
        .WithDuration(TimeSpan.FromSeconds(10))  // Party for 10 seconds
        .WithInterval(TimeSpan.FromMilliseconds(500))  // New kid every 0.5 seconds
        .RunAsync(async () =>
        {
            // Ask for a cookie!
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode; // Did I get my cookie?
        });

    // Check if kids got their cookies
    Assert.True(result.Success > 0, "At least some kids should get cookies!");
    Assert.True(result.AverageLatency < 2000, "Getting cookies shouldn't take too long!");
    Console.WriteLine($"🍪 {result.Total} kids asked for cookies, {result.Success} got them!");
}
```

#### Database Connection Pool Testing
```csharp
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.Extensions;

[Fact]
public async Task Test_Database_Connection_Pool()
{
    var connectionString = "Server=localhost;Database=TestDb;Integrated Security=true;";
    
    var result = await LoadTestRunner.Create()
        .WithName("DB_Connection_Pool")
        .WithConcurrency(50)           // Test connection pool limits
        .WithDuration(TimeSpan.FromMinutes(2))
        .WithInterval(TimeSpan.FromMilliseconds(100))
        .RunAsync(async () =>
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users";
            var count = await command.ExecuteScalarAsync();
            
            return count != null;
        });

    Assert.True(result.Failure < result.Success * 0.05, "Failure rate should be under 5%");
    Console.WriteLine($"Database test: {result.Success} successful connections");
}
```

### 🎭 Special Party Ending Rules (TerminationMode)

Think of these like different ways to end a birthday party:

#### 1. Quick Stop (Duration) ⏰
```csharp
using System;
using System.Threading.Tasks;
using Xunit;
using LoadSurge.Models;
using LoadSurge.Runner;

[Fact]
public async Task Test_Quick_Stop_Party()
{
    // Like mom saying "Party's over RIGHT NOW!"
    var plan = new LoadExecutionPlan
    {
        Name = "Quick_Stop_Party",
        Settings = new LoadSettings
        {
            Concurrency = 5,                    // 5 kids playing
            Duration = TimeSpan.FromSeconds(3), // 3-second party
            Interval = TimeSpan.FromMilliseconds(500), // New kid every 0.5s
            TerminationMode = TerminationMode.Duration  // Stop RIGHT NOW!
        },
        Action = async () =>
        {
            await Task.Delay(100); // Play time
            return true; // Had fun!
        }
    };

    var result = await LoadRunner.Run(plan);
    
    Assert.True(result.Success > 0);
    Console.WriteLine($"⚡ Quick stop: {result.Total} kids played");
}
```

#### 2. Nice Stop (CompleteCurrentInterval) 😊
```csharp
[Fact]
public async Task Test_Nice_Stop_Party()
{
    // Like saying "Finish your current game, then we stop!"
    var plan = new LoadExecutionPlan
    {
        Name = "Nice_Stop_Party",
        Settings = new LoadSettings
        {
            Concurrency = 5,
            Duration = TimeSpan.FromSeconds(3),
            Interval = TimeSpan.FromMilliseconds(500),
            TerminationMode = TerminationMode.CompleteCurrentInterval  // Let everyone finish!
        },
        Action = async () =>
        {
            await Task.Delay(100);
            return true;
        }
    };

    var result = await LoadRunner.Run(plan);
    
    // More kids get to finish their games!
    Assert.InRange(result.Total, 25, 35); // About 6 intervals × 5 kids
    Console.WriteLine($"😊 Nice stop: {result.Total} kids finished their games");
}
```

#### 3. Super Exact Stop (StrictDuration) ⏱️
```csharp
[Fact]
public async Task Test_Exact_Time_Party()
{
    // Like a timer that goes "BEEP!" at exactly 3 seconds
    var plan = new LoadExecutionPlan
    {
        Name = "Exact_Time_Party",
        Settings = new LoadSettings
        {
            Concurrency = 5,
            Duration = TimeSpan.FromSeconds(3),
            Interval = TimeSpan.FromMilliseconds(500),
            TerminationMode = TerminationMode.StrictDuration  // EXACTLY 3 seconds!
        },
        Action = async () =>
        {
            await Task.Delay(100);
            return true;
        }
    };

    var startTime = DateTime.UtcNow;
    var result = await LoadRunner.Run(plan);
    var actualTime = DateTime.UtcNow - startTime;
    
    // Should be very close to exactly 3 seconds!
    Assert.InRange(actualTime.TotalSeconds, 2.8, 3.3);
    Console.WriteLine($"⏱️ Exact time: Party lasted {actualTime.TotalSeconds:F1} seconds");
}
```

### ⏳ Waiting for Slow Friends (GracefulStopTimeout)

Sometimes kids take a long time to finish their games. This setting decides how long to wait:

```csharp
[Fact]
public async Task Test_Patient_Waiting()
{
    var plan = new LoadExecutionPlan
    {
        Name = "Patient_Waiting_Party",
        Settings = new LoadSettings
        {
            Concurrency = 5,
            Duration = TimeSpan.FromSeconds(3),
            Interval = TimeSpan.FromMilliseconds(200),
            GracefulStopTimeout = TimeSpan.FromSeconds(5),  // Wait 5 seconds for slow kids
            TerminationMode = TerminationMode.CompleteCurrentInterval
        },
        Action = async () =>
        {
            // Some kids are slow at games
            var delay = Random.Shared.Next(100, 2000); // Between 0.1 and 2 seconds
            await Task.Delay(delay);
            return true;
        }
    };

    var result = await LoadRunner.Run(plan);
    
    // Even slow kids should finish their games
    Assert.True(result.Success > result.Failure, "More kids should finish than give up");
    Console.WriteLine($"⏳ Patient waiting: {result.Success} kids finished, {result.Failure} were too slow");
}
```

### 🤖 Auto-Magic Waiting Time
The computer is smart and can figure out waiting time by itself!

```csharp
[Fact]
public async Task Test_Smart_Waiting_Time()
{
    // The computer calculates waiting time automatically!
    var settings = new LoadSettings
    {
        Concurrency = 5,
        Duration = TimeSpan.FromSeconds(30),  // 30-second party
        Interval = TimeSpan.FromSeconds(1)
        // No GracefulStopTimeout set - computer will use 9 seconds (30% of 30s)
    };

    // Check what the computer decided
    Assert.Equal(TimeSpan.FromSeconds(9), settings.EffectiveGracefulStopTimeout);
    Console.WriteLine($"🤖 Computer chose {settings.EffectiveGracefulStopTimeout.TotalSeconds} seconds waiting time");
    
    // For very short parties, computer uses minimum 5 seconds
    var shortParty = new LoadSettings
    {
        Duration = TimeSpan.FromSeconds(10),  // Short party
        Concurrency = 1,
        Interval = TimeSpan.FromSeconds(1)
    };
    Assert.Equal(TimeSpan.FromSeconds(5), shortParty.EffectiveGracefulStopTimeout);
    Console.WriteLine($"🕰️ For short parties, computer waits at least 5 seconds");
}
```
```

### 🏪 Real Party Examples

#### Ice Cream Shop Test 🍦
```csharp
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.Extensions;

[Fact]
public async Task Test_Ice_Cream_Shop_Rush()
{
    using var httpClient = new HttpClient();
    var flavors = new[] { "vanilla", "chocolate", "strawberry", "mint" };
    
    var result = await LoadTestRunner.Create()
        .WithName("Ice_Cream_Shop_Rush")
        .WithConcurrency(15)        // 15 kids want ice cream!
        .WithDuration(TimeSpan.FromMinutes(2))  // 2-minute ice cream rush
        .WithInterval(TimeSpan.FromMilliseconds(300))  // New kid every 0.3 seconds
        .RunAsync(async () =>
        {
            // Pick a random flavor
            var flavor = flavors[Random.Shared.Next(flavors.Length)];
            
            // Ask for ice cream
            var response = await httpClient.GetAsync($"https://jsonplaceholder.typicode.com/posts/{Random.Shared.Next(1, 10)}");
            if (!response.IsSuccessStatusCode) return false;
            
            // Yummy! Time to eat it
            await Task.Delay(Random.Shared.Next(100, 300));
            
            return true; // Got my ice cream!
        });

    // Check if the ice cream shop handled the rush well
    Assert.True(result.Success > result.Total * 0.9, "90% of kids should get ice cream!");
    Assert.True(result.AverageLatency < 1000, "Getting ice cream shouldn't take too long!");
    
    Console.WriteLine($"🍦 Ice Cream Shop Results:");
    Console.WriteLine($"   Total kids: {result.Total}");
    Console.WriteLine($"   Got ice cream: {result.Success} 😋");
    Console.WriteLine($"   No ice cream: {result.Failure} 😢");
    Console.WriteLine($"   Average wait time: {result.AverageLatency:F0} milliseconds");
    Console.WriteLine($"   Ice creams per second: {result.RequestsPerSecond:F1}");
}
```

#### Playground Safety Test 🛝
```csharp
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using LoadSurge.Models;
using LoadSurge.Runner;

[Fact]
public async Task Test_Playground_Safety()
{
    using var httpClient = new HttpClient();
    var playgroundBroken = false;
    
    var plan = new LoadExecutionPlan
    {
        Name = "Playground_Safety_Test",
        Settings = new LoadSettings
        {
            Concurrency = 30,  // LOTS of kids on playground!
            Duration = TimeSpan.FromSeconds(20),
            Interval = TimeSpan.FromMilliseconds(100), // Kids arrive fast!
            TerminationMode = TerminationMode.CompleteCurrentInterval,
            GracefulStopTimeout = TimeSpan.FromSeconds(10)
        },
        Action = async () =>
        {
            try
            {
                // Try to use the playground equipment
                var response = await httpClient.GetAsync(
                    "https://jsonplaceholder.typicode.com/posts/1",
                    new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token
                );
                
                return response.IsSuccessStatusCode; // Did I get to play?
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("playground broken"))
            {
                playgroundBroken = true;
                return false;  // Playground is broken!
            }
            catch (TaskCanceledException)
            {
                return false;  // Took too long
            }
        }
    };

    var result = await LoadRunner.Run(plan);
    
    Console.WriteLine($"🛝 Playground Safety Test:");
    Console.WriteLine($"   Kids who played: {result.Success} 😊");
    Console.WriteLine($"   Kids who couldn't play: {result.Failure} 😞");
    Console.WriteLine($"   Playground broken? {playgroundBroken}");
    
    // Make sure playground can handle lots of kids
    Assert.True(result.Success > 0, "Some kids should be able to play!");
}
```

### 🎯 Smart Tips for Great Parties

#### Start Small, Then Make Bigger Parties 📈
```csharp
using System;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.Extensions;

// Baby party - just a few friends
[Fact]
public async Task Baby_Party_Test()
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(2)  // Just 2 friends
        .WithDuration(TimeSpan.FromSeconds(5))  // Quick party
        .WithInterval(TimeSpan.FromSeconds(1))
        .RunAsync(async () => await PlayGame());
    
    Assert.True(result.Success > 0);
    Console.WriteLine($"👶 Baby party: {result.Success} friends had fun!");
}

// Medium party - more friends!
[Fact]
public async Task Medium_Party_Test()
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(10)  // 10 friends
        .WithDuration(TimeSpan.FromMinutes(2))  // Longer party
        .WithInterval(TimeSpan.FromMilliseconds(500))
        .RunAsync(async () => await PlayGame());
        
    Console.WriteLine($"🎈 Medium party: {result.Success} friends had fun!");
}

// BIG party - LOTS of friends!
[Fact]
public async Task Big_Party_Test()
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(50)  // 50 friends! WOW!
        .WithDuration(TimeSpan.FromMinutes(10))  // Long party
        .WithInterval(TimeSpan.FromMilliseconds(200))
        .RunAsync(async () => await PlayGame());
        
    Console.WriteLine($"🎊 BIG party: {result.Success} friends had AMAZING fun!");
}

private async Task<bool> PlayGame()
{
    // Pretend to play a game
    await Task.Delay(100);
    return true; // Had fun!
}
```

#### Pick the Best Party Ending 🎭
```csharp
using LoadSurge.Models;

// For counting how many kids played - use Nice Stop (recommended)
var niceParty = new LoadSettings
{
    TerminationMode = TerminationMode.CompleteCurrentInterval, // Let everyone finish!
    // ... other party settings
};

// For quick parties - use Quick Stop
var quickParty = new LoadSettings
{
    TerminationMode = TerminationMode.Duration,  // Stop right now!
    // ... other party settings
};

// For exact timing - use Super Exact Stop
var exactParty = new LoadSettings
{
    TerminationMode = TerminationMode.StrictDuration, // EXACTLY on time!
    // ... other party settings
};

Console.WriteLine("🎭 Pick the party ending that works best for your test!");
```
```

## Want to see what's happening? Use logs! 📝

## 📝 Want to See What's Happening? 

Add this magic spell to see everything your tests are doing:

```csharp
// Add to your test project
services.AddOTelDiagnostics();
```

It's like having a special notebook that writes down everything that happens during your party!

## 🎯 What Do All These Numbers Mean?

After your party test, you get a report card! Here's what each number tells you:

- **Total** 📊: How many kids came to your party
- **Success** ✅: How many kids had fun
- **Failure** ❌: How many kids were sad or couldn't play
- **AverageLatency** ⏱️: How long it took on average to get cookies/toys/fun stuff
- **MinLatency/MaxLatency** 🏃‍♀️🐌: The fastest and slowest kid to get their stuff
- **RequestsPerSecond** ⚡: How many kids per second got what they wanted
- **Time** 🕐: How long the whole party lasted

## 🎮 What You Need to Play

- **.NET 8.0** or newer (like having the newest game console)
- **xUnit v3** for testing (the special testing toy box)

## 🆘 Need Help?

- 🐛 **Something's broken?**: [Tell us here](https://github.com/mrviduus/xUnitV3LoadFramework/issues)
- 💬 **Want to chat?**: [Let's talk here](https://github.com/mrviduus/xUnitV3LoadFramework/discussions)
- 📧 **Send a letter**: [mrviduus@gmail.com](mailto:mrviduus@gmail.com)

---

**Made with lots of love ❤️ by [Vasyl](https://github.com/mrviduus) to help make everyone's apps super strong and fast!**
