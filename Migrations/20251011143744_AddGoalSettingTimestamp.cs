using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessNutritionTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalSettingTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "GoalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.Now);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "GoalSettings");
        }
    }
}
