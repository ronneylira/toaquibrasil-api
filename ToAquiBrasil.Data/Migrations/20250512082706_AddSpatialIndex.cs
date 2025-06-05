using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToAquiBrasil.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First drop the regular index if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.indexes 
                    WHERE name = 'IX_Listings_Location' 
                    AND object_id = OBJECT_ID('Listings')
                ) 
                DROP INDEX IX_Listings_Location ON Listings;
            ");

            // Create spatial index with optimized parameters
            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX IX_Listings_Location_Spatial 
                ON Listings(Location)
                USING GEOGRAPHY_AUTO_GRID
                WITH (
                    CELLS_PER_OBJECT = 16,
                    PAD_INDEX = OFF,
                    STATISTICS_NORECOMPUTE = OFF,
                    SORT_IN_TEMPDB = OFF,
                    DROP_EXISTING = OFF,
                    ONLINE = OFF,
                    ALLOW_ROW_LOCKS = ON,
                    ALLOW_PAGE_LOCKS = ON
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // First drop the regular index if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.indexes 
                    WHERE name = 'IX_Listings_Location' 
                    AND object_id = OBJECT_ID('Listings')
                ) 
                DROP INDEX IX_Listings_Location ON Listings;
            ");
        }
    }
}
