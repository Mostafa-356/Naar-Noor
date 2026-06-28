using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Tests.Fixtures;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Persistence.Repositories;

/// <summary>
/// Property-based tests for repository CRUD operations.
/// These tests validate that Create, Read, Update, and Delete operations
/// work correctly in round-trip scenarios, maintaining data consistency.
/// 
/// **Validates: Requirements 3.1**
/// **Property 7: CRUD Round-Trip Operations**
/// </summary>
public class RepositoryCrudPropertyTests : RepositoryTestBase
{
    /// <summary>
    /// Property: Create operation with valid data persists correctly.
    /// For any valid Chef entity, when created and saved to the database,
    /// the entity SHALL be retrievable with all fields intact and matching the original.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CreateOperation_WithValidData_PersistsCorrectly()
    {
        return Prop.ForAll(
            Arb.From(GenerateValidChefs()),
            chef =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange
                    var chefToCreate = chef;
                    
                    // Act - Create
                    DbContext.Chefs.Add(chefToCreate);
                    await DbContext.SaveChangesAsync();
                    
                    var chefId = chefToCreate.Id;
                    
                    // Assert - verify in fresh context
                    await using var freshContext = await CreateIsolatedDbContextAsync();
                    var retrievedChef = await freshContext.Chefs.FirstAsync(c => c.Id == chefId);
                    
                    return retrievedChef.Name == chefToCreate.Name &&
                           retrievedChef.Email == chefToCreate.Email &&
                           retrievedChef.PhoneNumber == chefToCreate.PhoneNumber &&
                           retrievedChef.Specialization == chefToCreate.Specialization;
                });
                
                return task;
            }
        );
    }

    /// <summary>
    /// Property: Read operation returns correct entity by ID.
    /// For any Chef entity in the database, when queried by its ID,
    /// the retrieved entity SHALL match the persisted data exactly.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ReadOperation_ById_ReturnsCorrectEntity()
    {
        return Prop.ForAll(
            Arb.From(GenerateValidChefs()),
            chef =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange - Create and persist
                    DbContext.Chefs.Add(chef);
                    await DbContext.SaveChangesAsync();
                    var chefId = chef.Id;
                    
                    // Clear tracking to ensure fresh read
                    DbContext.ChangeTracker.Clear();
                    
                    // Act - Read
                    var retrievedChef = await DbContext.Chefs
                        .FirstOrDefaultAsync(c => c.Id == chefId);
                    
                    // Assert
                    if (retrievedChef == null)
                        return false;
                    
                    return retrievedChef.Id == chef.Id &&
                           retrievedChef.Name == chef.Name &&
                           retrievedChef.Email == chef.Email;
                });
                
                return task;
            }
        );
    }

    /// <summary>
    /// Property: Update operation persists changes to existing entity.
    /// For any Chef entity in the database, when its properties are modified
    /// and saved, the database SHALL reflect all changes on subsequent reads.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UpdateOperation_ModifiesProperties_PersistsChanges()
    {
        return Prop.ForAll(
            Arb.From(GenerateValidChefPairs()),
            chefPair =>
            {
                var task = RunAsync(async () =>
                {
                    var (originalChef, updatedChef) = chefPair;
                    
                    // Arrange - Create initial chef
                    DbContext.Chefs.Add(originalChef);
                    await DbContext.SaveChangesAsync();
                    var chefId = originalChef.Id;
                    
                    // Act - Update
                    var toUpdate = await DbContext.Chefs.FirstAsync(c => c.Id == chefId);
                    toUpdate.Name = updatedChef.Name;
                    toUpdate.Email = updatedChef.Email;
                    toUpdate.PhoneNumber = updatedChef.PhoneNumber;
                    
                    DbContext.Chefs.Update(toUpdate);
                    await DbContext.SaveChangesAsync();
                    
                    // Assert - Verify in fresh context
                    await using var freshContext = await CreateIsolatedDbContextAsync();
                    var retrievedChef = await freshContext.Chefs.FirstAsync(c => c.Id == chefId);
                    
                    return retrievedChef.Name == updatedChef.Name &&
                           retrievedChef.Email == updatedChef.Email &&
                           retrievedChef.PhoneNumber == updatedChef.PhoneNumber;
                });
                
                return task;
            }
        );
    }

    /// <summary>
    /// Property: Delete operation removes entity from database.
    /// For any Chef entity in the database, when deleted and saved,
    /// subsequent queries for that entity SHALL return no results.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property DeleteOperation_RemovesEntity_DoesNotExistAfterDelete()
    {
        return Prop.ForAll(
            Arb.From(GenerateValidChefs()),
            chef =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange - Create and persist
                    DbContext.Chefs.Add(chef);
                    await DbContext.SaveChangesAsync();
                    var chefId = chef.Id;
                    
                    // Act - Delete
                    var toDelete = await DbContext.Chefs.FirstAsync(c => c.Id == chefId);
                    DbContext.Chefs.Remove(toDelete);
                    await DbContext.SaveChangesAsync();
                    
                    // Assert - Verify in fresh context
                    await using var freshContext = await CreateIsolatedDbContextAsync();
                    var deletedChef = await freshContext.Chefs
                        .FirstOrDefaultAsync(c => c.Id == chefId);
                    
                    return deletedChef == null;
                });
                
                return task;
            }
        );
    }

    /// <summary>
    /// Property: Round-trip creates then reads correctly.
    /// For any Chef entity, a complete round-trip of create followed by immediate read
    /// SHALL return the exact same data, validating end-to-end persistence.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RoundTrip_Create_Then_Read_ReturnsIdenticalData()
    {
        return Prop.ForAll(
            Arb.From(GenerateValidChefs()),
            originalChef =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange
                    var chefToCreate = Chef.Create(
                        name: originalChef.Name,
                        email: originalChef.Email,
                        phoneNumber: originalChef.PhoneNumber,
                        specialization: originalChef.Specialization,
                        yearsOfExperience: originalChef.YearsOfExperience
                    );
                    
                    // Act - Create
                    DbContext.Chefs.Add(chefToCreate);
                    await DbContext.SaveChangesAsync();
                    
                    var createdId = chefToCreate.Id;
                    
                    // Clear tracking
                    DbContext.ChangeTracker.Clear();
                    
                    // Act - Read
                    var readChef = await DbContext.Chefs.FirstAsync(c => c.Id == createdId);
                    
                    // Assert - all fields match
                    return readChef.Id == chefToCreate.Id &&
                           readChef.Name == chefToCreate.Name &&
                           readChef.Email == chefToCreate.Email &&
                           readChef.PhoneNumber == chefToCreate.PhoneNumber &&
                           readChef.Specialization == chefToCreate.Specialization &&
                           readChef.YearsOfExperience == chefToCreate.YearsOfExperience;
                });
                
                return task;
            }
        );
    }

    /// <summary>
    /// Property: Multiple CRUD operations maintain data consistency.
    /// For a sequence of create, read, update, and delete operations on multiple Chef entities,
    /// the database state SHALL remain consistent with no data loss or corruption.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MultipleCrudOperations_MaintainDataConsistency()
    {
        return Prop.ForAll(
            Arb.From(GenerateValidChefLists(count: 5)),
            chefs =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange
                    var chefList = chefs.ToList();
                    
                    // Act - Create multiple
                    foreach (var chef in chefList)
                    {
                        DbContext.Chefs.Add(chef);
                    }
                    await DbContext.SaveChangesAsync();
                    
                    var ids = chefList.Select(c => c.Id).ToList();
                    
                    // Act - Read all
                    var retrieved = await DbContext.Chefs
                        .Where(c => ids.Contains(c.Id))
                        .ToListAsync();
                    
                    if (retrieved.Count != chefList.Count)
                        return false;
                    
                    // Act - Update first
                    if (retrieved.Any())
                    {
                        retrieved[0].Name = "Updated Name";
                        DbContext.Chefs.Update(retrieved[0]);
                        await DbContext.SaveChangesAsync();
                    }
                    
                    // Act - Delete last
                    if (retrieved.Count > 1)
                    {
                        DbContext.Chefs.Remove(retrieved.Last());
                        await DbContext.SaveChangesAsync();
                    }
                    
                    // Assert - Verify final state in fresh context
                    await using var freshContext = await CreateIsolatedDbContextAsync();
                    var finalCount = await freshContext.Chefs
                        .Where(c => ids.Contains(c.Id))
                        .CountAsync();
                    
                    var expectedCount = chefList.Count - 1; // One was deleted
                    return finalCount == expectedCount;
                });
                
                return task;
            }
        );
    }

    /// <summary>
    /// Property: Create with minimal valid data succeeds.
    /// For any Chef with required fields populated, creation and persistence
    /// SHALL succeed even with minimal optional data.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CreateWithMinimalData_Succeeds()
    {
        return Prop.ForAll(
            Arb.Default.String().Where(s => !string.IsNullOrEmpty(s) && s.Length < 100),
            name =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange
                    var chef = Chef.Create(
                        name: name,
                        email: "chef@example.com",
                        phoneNumber: "555-0000",
                        specialization: "General",
                        yearsOfExperience: 1
                    );
                    
                    // Act
                    DbContext.Chefs.Add(chef);
                    await DbContext.SaveChangesAsync();
                    
                    // Assert
                    await using var freshContext = await CreateIsolatedDbContextAsync();
                    var created = await freshContext.Chefs.FirstOrDefaultAsync(c => c.Id == chef.Id);
                    
                    return created != null && created.Name == name;
                });
                
                return task;
            }
        );
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Generator for valid Chef entities.
    /// </summary>
    private IEnumerable<Chef> GenerateValidChefs()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return Chef.Create(
                name: $"Chef {i}",
                email: $"chef{i}@example.com",
                phoneNumber: $"555-{i:D4}",
                specialization: GetSpecialization(i),
                yearsOfExperience: i + 1
            );
        }
    }

    /// <summary>
    /// Generator for pairs of Chef entities (original and updated).
    /// </summary>
    private IEnumerable<(Chef Original, Chef Updated)> GenerateValidChefPairs()
    {
        for (int i = 0; i < 10; i++)
        {
            var original = Chef.Create(
                name: $"Chef {i}",
                email: $"chef{i}@example.com",
                phoneNumber: $"555-{i:D4}",
                specialization: GetSpecialization(i),
                yearsOfExperience: i + 1
            );
            
            var updated = Chef.Create(
                name: $"Updated Chef {i}",
                email: $"updated{i}@example.com",
                phoneNumber: $"555-{i + 1:D4}",
                specialization: GetSpecialization((i + 1) % 3),
                yearsOfExperience: i + 2
            );
            
            yield return (original, updated);
        }
    }

    /// <summary>
    /// Generator for lists of Chef entities.
    /// </summary>
    private IEnumerable<List<Chef>> GenerateValidChefLists(int count)
    {
        for (int seed = 0; seed < 5; seed++)
        {
            var list = new List<Chef>();
            for (int i = 0; i < count; i++)
            {
                list.Add(Chef.Create(
                    name: $"Chef {seed}-{i}",
                    email: $"chef{seed}{i}@example.com",
                    phoneNumber: $"555-{i:D4}",
                    specialization: GetSpecialization(i),
                    yearsOfExperience: i + 1
                ));
            }
            yield return list;
        }
    }

    /// <summary>
    /// Helper to get specialization by index.
    /// </summary>
    private string GetSpecialization(int index) =>
        index % 3 switch
        {
            0 => "French",
            1 => "Italian",
            _ => "Asian"
        };

    /// <summary>
    /// Helper to run async code synchronously for property-based tests.
    /// </summary>
    private bool RunAsync(Func<Task<bool>> func)
    {
        try
        {
            return func().Result;
        }
        catch
        {
            return false;
        }
    }
}
