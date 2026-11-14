using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parko.Migrations
{
    /// <inheritdoc />
    public partial class ValidationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_VipPoints_Points_Range",
                table: "VipPoints",
                sql: "[Points] >= 0 AND [Points] <= 40");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VipCarRecords_Vehicle_Length",
                table: "VipCarRecords",
                sql: "DATALENGTH([Vehicle]) >= 8 AND DATALENGTH([Vehicle]) <= 24");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VipCarRecords_Year_Range",
                table: "VipCarRecords",
                sql: "[Year] >= 1950 AND [Year] <= 2100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParkingSpaces_Id_Range",
                table: "ParkingSpaces",
                sql: "[Id] >= 1 AND [Id] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clients_Email_Length",
                table: "Clients",
                sql: "DATALENGTH([Email]) >= 2 AND DATALENGTH([Email]) <= 508");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clients_Phone_Length",
                table: "Clients",
                sql: "DATALENGTH([Phone]) >= 2 AND DATALENGTH([Phone]) <= 32");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cars_Model_Length",
                table: "Cars",
                sql: "DATALENGTH([Model]) >= 2 AND DATALENGTH([Model]) <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cars_Price_NonNegative",
                table: "Cars",
                sql: "[Price] IS NULL OR [Price] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cars_Vehicle_Length",
                table: "Cars",
                sql: "DATALENGTH([Vehicle]) >= 8 AND DATALENGTH([Vehicle]) <= 24");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VipPoints_Points_Range",
                table: "VipPoints");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VipCarRecords_Vehicle_Length",
                table: "VipCarRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VipCarRecords_Year_Range",
                table: "VipCarRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ParkingSpaces_Id_Range",
                table: "ParkingSpaces");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clients_Email_Length",
                table: "Clients");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clients_Phone_Length",
                table: "Clients");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cars_Model_Length",
                table: "Cars");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cars_Price_NonNegative",
                table: "Cars");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cars_Vehicle_Length",
                table: "Cars");
        }
    }
}
