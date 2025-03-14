﻿using Lapo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Lapo.Integration.Tests;

[TestClass]
public class ConfigurationServiceTests
{
    const string Path = "appsettings.test.json";
    const string Section = "Test";

    [TestInitialize]
    public async Task SetUp()
    {
        if (File.Exists(Path)) File.Delete(Path);
        await File.WriteAllTextAsync(Path, "{}");
    }

    [TestCleanup]
    public void TearDown()
    {
        if (File.Exists(Path)) File.Delete(Path);
    }

    [TestMethod]
    public async Task UpsertAsync_ShouldAddNewValue()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("NewKey", "NewValue");

        const string expected = """
                                {
                                  "Test": {
                                    "NewKey": "NewValue"
                                  }
                                }
                                """;

        var actual = await File.ReadAllTextAsync(Path);
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task UpsertAsync_ShouldUpdateExistingValue()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("ExistingKey", "InitialValue");
        await sut.UpsertAsync("ExistingKey", "UpdatedValue");

        const string expected = """
                                {
                                  "Test": {
                                    "ExistingKey": "UpdatedValue"
                                  }
                                }
                                """;

        var actual = await File.ReadAllTextAsync(Path);
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveExistingValue()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("Key", "Value");
        await sut.RemoveAsync("Key");

        const string expected = """
                                {
                                  "Test": {}
                                }
                                """;

        var actual = await File.ReadAllTextAsync(Path);
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveNestedKey()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("SubSection:SubSubSection:Key", "NestedValue");
        await sut.RemoveAsync("SubSection:SubSubSection:Key");

        const string expected = """
                                {
                                  "Test": {
                                    "SubSection": {
                                      "SubSubSection": {}
                                    }
                                  }
                                }
                                """;

        var actual = await File.ReadAllTextAsync(Path);
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveParentSectionIfEmpty()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("SubSection:SubSubSection:Key", "Value");
        await sut.RemoveAsync("SubSection:SubSubSection:Key");

        const string expected = """
                                {
                                  "Test": {
                                    "SubSection": {
                                      "SubSubSection": {}
                                    }
                                  }
                                }
                                """;

        var actual = await File.ReadAllTextAsync(Path);
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldDoNothingIfKeyDoesNotExist()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("Key", "Value");
        await sut.RemoveAsync("NonExistentKey");

        const string expected = """
                                {
                                  "Test": {
                                    "Key": "Value"
                                  }
                                }
                                """;

        var actual = await File.ReadAllTextAsync(Path);
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public void Constructor_ShouldThrowFileNotFoundExceptionIfFileDoesNotExist()
    {
        ThrowsExactly<FileNotFoundException>(() => _ = new ConfigurationService("nonexistent.json", "Section"));
    }
}