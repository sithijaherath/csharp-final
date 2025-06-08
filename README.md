# File Indexing System

A distributed system for indexing words in text files using multiple processes and named pipes for communication.

## Project Structure

The solution consists of three console applications:

1. **ScannerA**: First agent responsible for scanning text files in a specified directory
2. **ScannerB**: Second agent responsible for scanning text files in a specified directory
3. **Master**: Central process that receives and aggregates data from both scanners

## Features

- Distributed file processing using multiple processes
- Named pipe communication between processes
- CPU core affinity configuration for each process
- Multithreaded file reading and data processing
- Word counting and aggregation functionality

## Requirements

- .NET 6.0 or later
- Windows OS (for named pipes)

## How to Run

1. Build the solution:
   ```
   dotnet build
   ```

2. Run the Master process first:
   ```
   dotnet run --project Master
   ```

3. Run ScannerA with a directory path:
   ```
   dotnet run --project ScannerA "path/to/directory"
   ```

4. Run ScannerB with a directory path:
   ```
   dotnet run --project ScannerB "path/to/directory"
   ```

## Implementation Details

- Each scanner process runs on a separate CPU core (ScannerA on core 0, ScannerB on core 1, Master on core 2)
- Named pipes are used for inter-process communication
- The Master process aggregates word counts from both scanners
- Results are displayed in alphabetical order by word

## Notes

- Make sure to run the Master process before starting the scanners
- The scanners will wait for up to 5 seconds to connect to the Master process
- Text files should be in the specified directories for processing 