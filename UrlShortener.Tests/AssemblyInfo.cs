using Xunit;

// Disables parallel test execution to prevent race conditions when multiple WebApplicationFactory instances run migrations simultaneously
// Without this, multiple WebApplicationFactory instances may attempt to apply EF Core migrations concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
