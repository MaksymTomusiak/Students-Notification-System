using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedCourseBans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "course_bans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "varchar(255)", nullable: false),
                    banned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_bans", x => x.id);
                    table.ForeignKey(
                        name: "fk_course_bans_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_course_bans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_role_guid_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_roles_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_roles_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_role_guid_user", x => new { x.user_id, x.user_roles_user_id, x.user_roles_role_id });
                    table.ForeignKey(
                        name: "fk_identity_user_role_guid_user_user_roles_user_roles_user_id_",
                        columns: x => new { x.user_roles_user_id, x.user_roles_role_id },
                        principalTable: "AspNetUserRoles",
                        principalColumns: new[] { "user_id", "role_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_user_role_guid_user_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_course_bans_course_id",
                table: "course_bans",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_course_bans_user_id",
                table: "course_bans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_role_guid_user_user_roles_user_id_user_roles_",
                table: "identity_user_role_guid_user",
                columns: new[] { "user_roles_user_id", "user_roles_role_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_bans");

            migrationBuilder.DropTable(
                name: "identity_user_role_guid_user");
        }
    }
}
