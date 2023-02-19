using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace QifWriter.Tests;

public class QifWriterTests
{
    [Fact]
    public void WriteFile_WhenTransactionsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var writer = new QifWriter(fileSystem);

        // Act
        Action act = () => writer.WriteFile("test.qif", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("transactions");
    }

    [Fact]
    public void WriteFile_WhenFileNameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var writer = new QifWriter(fileSystem);

        // Act
        Action act = () => writer.WriteFile(null!, Array.Empty<QifTransaction>());

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileName");
    }

    [Fact]
    public void WriteFile_WhenCalled_WritesFileToDisk()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var writer = new QifWriter(fileSystem);

        var transactions = new[]
        {
            new QifTransaction
            {
                Type = QifTransactionType.Checking,
                Date = DateOnly.Parse("2021-01-01"),
                Amount = 100.50m,
                Payee = "Test Payee",
                Category = "Test Category",
                SubTransactions = new List<QifTransaction.SubTransaction>
                {
                    new()
                    {
                        Category = "Test Sub Category 1",
                        Extra = "Test Sub Extra 1",
                        Amount = 10.0m
                    },
                    new()
                    {
                        Category = "Test Sub Category 2",
                        Extra = "Test Sub Extra 2",
                        Amount = 20.0m
                    }
                },
                Memo = "Test Memo"
            }
        };

        // Act
        writer.WriteFile("test.qif", transactions);

        // Assert
        fileSystem.FileExists("test.qif").Should().BeTrue();

        string fileContents = fileSystem.File.ReadAllText("test.qif");
        fileContents.Should().Contain($"Type:{transactions.First().Type.GetType().Name}")
            .And.Contain($"D:{transactions.First().Date:yyyy-MM-dd}")
            .And.Contain("T:100.50")
            .And.Contain("P:Test Payee")
            .And.Contain("L:Test Category:Test Sub Category 1")
            .And.Contain("S:Test Category:Test Sub Category 1")
            .And.Contain("E:Test Sub Extra 1")
            .And.Contain("$10.00")
            .And.Contain("S:Test Category:Test Sub Category 2")
            .And.Contain("E:Test Sub Extra 2")
            .And.Contain("$20.00")
            .And.Contain("M:Test Memo")
            .And.Contain("^");
    }
    
    [Fact]
    public void WriteFile_WhenCalled_WritesFileToDiskForManyTransactions()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var writer = new QifWriter(fileSystem);

        var transactions = new[]
        {
            new QifTransaction
            {
                Type = QifTransactionType.Checking,
                Date = DateOnly.Parse("2021-01-01"),
                Amount = 100.50m,
                Payee = "Test Payee",
                Category = "Test Category",
                SubTransactions = new List<QifTransaction.SubTransaction>
                {
                    new()
                    {
                        Category = "Test Sub Category 1",
                        Extra = "Test Sub Extra 1",
                        Amount = 10.0m
                    },
                    new()
                    {
                        Category = "Test Sub Category 2",
                        Extra = "Test Sub Extra 2",
                        Amount = 20.0m
                    }
                },
                Memo = "Test Memo"
            },
            new QifTransaction
            {
                Type = QifTransactionType.Checking,
                Date = DateOnly.Parse("2021-01-02"),
                Amount = 50.50m,
                Payee = "Test Payee",
                Category = "Test Category",
                SubTransactions = new List<QifTransaction.SubTransaction>
                {
                    new()
                    {
                        Category = "Test Sub Category 1",
                        Extra = "Test Sub Extra 1",
                        Amount = 30.0m
                    },
                    new()
                    {
                        Category = "Test Sub Category 2",
                        Extra = "Test Sub Extra 2",
                        Amount = 40.0m
                    }
                },
                Memo = "Test Memo"
            }
        };

        // Act
        writer.WriteFile("test.qif", transactions);

        // Assert
        fileSystem.FileExists("test.qif").Should().BeTrue();

        string fileContents = fileSystem.File.ReadAllText("test.qif");
        fileContents.Should()
            .Contain($"Type:{transactions.First().Type.GetType().Name}")
            .And.Contain($"D:{transactions.First().Date:yyyy-MM-dd}")
            .And.Contain("T:100.50")
            .And.Contain("P:Test Payee")
            .And.Contain("L:Test Category:Test Sub Category 1")
            .And.Contain("S:Test Category:Test Sub Category 1")
            .And.Contain("E:Test Sub Extra 1")
            .And.Contain("$10.00")
            .And.Contain("S:Test Category:Test Sub Category 2")
            .And.Contain("E:Test Sub Extra 2")
            .And.Contain("$20.00")
            .And.Contain("M:Test Memo")
            .And.Contain("^")
            .And.Contain($"D:{transactions.Skip(1).First().Date:yyyy-MM-dd}")
            .And.Contain("T:50.50")
            .And.Contain("$30.00")
            .And.Contain("$40.00");
    }
}