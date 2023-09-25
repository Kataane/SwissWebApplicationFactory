namespace SwissWebApplicationFactory.Tests.Base;

[CollectionDefinition(nameof(SwissWebApplicationFactoryCollection))]
public class SwissWebApplicationFactoryCollection : ICollectionFixture<SwissWebApplicationFactory<Program>> { }