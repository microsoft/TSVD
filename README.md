# What is TSVD

TSVD is a thread-safety violation detecting tool designed in the paper "Efficient and Scalable Thread-Safety Violation Detection --- Finding thousands of concurrency bugs during testing" in SOSP2019.

## What is thread-safety violation

Some research results discover that it is fairly common that developers erroneously assume some concurrent accesses on different part of a data structure is thread safe. For example :

    //Dictionary dict
    Thread1: dict.Add(key1,value);
    Thread2: dict.ContainsKey(key2);
    
Even key1 is different with key2, it is still possible to lead a undetermined result.

### What kind of class is not thread-safe

In C#, most classes in System.Collections are thread-unsafe unless they are protected by a specific lock.

## How to apply TSVD

### Overview

There are only three easy steps to adopt TSVD for your software.

+ Compile the TSVD source code. It generates a `TSVDInstrumenter.exe` in `src/TSVDInstrumenter/bin/Debug`.
+ Instrucment the testing binaries with `TSVDInstrumenter.exe`. The usage of `TSVDInstrumenter.exe` is:

    `.\TSVDInstrumenter.exe [directory to the testing binary] [path to instrument configuration] [path to runtime configuration]`

+ Run the test as normal.

### Example

TestApps/DataRace is an example to demonstrate how to apply TSVD:

+ Preparion. Compile the DataRace source code. It generates a DataRace.exe in `DataRace/bin/Debug`. 
+ Instrumentation. Under DataRace/bin/Debug, run the following command in command line:

    `& ..\..\..\..\src\TSVDInstrumenter\bin\Debug\TSVDInstrumenter.exe . .\Configurations\instrumentation-config.cfg .\Configurations\runtime-config.cfg`. It instruments all the binaries in the current folder. Since there is only one exe/dll in the current folder, it will only instrument `DataRace.exe`.

+ Run. After observing the `Instrumentation result: OK` which indicates `DataRace.exe` is already instrumented, run `.\DataRace.exe`. Since running the insturmented `DataRace.exe`, it writes all the detected thread-safety violation to the `TSVD-bug-*.tsvdlog` file.
    

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
