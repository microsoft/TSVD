# What is TSVD

TSVD is a thread-safety violation detection tool described in the paper "Efficient and Scalable Thread-Safety Violation Detection --- Finding thousands of concurrency bugs during testing" in SOSP 2019.

## What is thread-safety violation

A thread-safety violation occurs when an application concurrently calls into a library/class in a way that violates its thread-safety contract. For example, in .NET, `System.Collections.Generic.Dictionary` is not thread-safe for concurrent accesses when one or both accesses are write operations. Therefore, the following two concurrent operations are not thread-safe.

    //Dictionary dict
    Thread1: dict.Add(key1,value);
    Thread2: dict.ContainsKey(key2);
    
The above two concurrent operations can give nondeterministic results or corrupt the data structure, even if `key1` and `key2` are different.

### What kind of class is not thread-safe

In C#, most classes under the `System.Collections` namespace are thread-unsafe unless they are protected by a specific lock.

## How to apply TSVD

### Overview

`TSVD` is designed for .NET applications. It works by instrumenting application binaries and running existing workloads/tests on instrumented binaries. Instrumentation is done by tool called `TSVDInstrumenter.exe`. Compiling the source code in this repo produces `TSVDInstrumenter.exe` in `src/TSVDInstrumenter/bin/Debug`.

As mentioned, applying `TSVD` on an application involves two steps:  

+ Instrument the testing binaries with `TSVDInstrumenter.exe`. The usage of `TSVDInstrumenter.exe` is:

    `.\TSVDInstrumenter.exe [directory containing applicaiton binaries] [path to instrumentation configuration] [path to runtime configuration]`

	The instrumentation configuration file dictates `TSVD`'s instrumentation behavior (e.g., what thread-unsafe APIs are instrumented), while the runtime configuration file contains `TSVD`'s runtime behavior (e.g., what algorithm to use, where to write bug log files, etc.) 

+ Run instrumented binaries with existing workloads/tests. If any thread-safety violation is detected, `TSVD` will produce a file named `TSVD-bug-*.tsvdlog` that contains details of the violation.

### Example

The repo contains an example application `TestApps/DataRace` that includes several thread-safety violations (concurrently sorting the same `List`). `TSVD` can be applied on `DataRace.exe` as follows:

+ Instrumentation. Under DataRace/bin/Debug, run the following command in command line:

    `& ..\..\..\..\src\TSVDInstrumenter\bin\Debug\TSVDInstrumenter.exe . .\Configurations\instrumentation-config.cfg .\Configurations\runtime-config.cfg`. It instruments all the binaries (in this case, only `DataRace.exe`) in the current folder. It will also copy `TSVD` runtime library and runtime configuration file to the current folder.

+ Run. After observing the `Instrumentation result: OK` which indicates `DataRace.exe` is already instrumented, run `.\DataRace.exe`. Since running the insturmented `DataRace.exe`, it writes all the detected thread-safety violation to the `TSVD-bug-*.tsvdlog` file.
    
### Feedback/Questions
For any question about the tool, please email us at `tsvd_at_microsoft_dot_com`

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
