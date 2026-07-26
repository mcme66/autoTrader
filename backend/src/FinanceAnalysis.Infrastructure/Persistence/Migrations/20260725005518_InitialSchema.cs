using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceAnalysis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_sources",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ml_models",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ml_models", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sectors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sectors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    data_source_id = table.Column<int>(type: "integer", nullable: false),
                    trade_date = table.Column<DateOnly>(type: "date", nullable: true),
                    range_start = table.Column<DateOnly>(type: "date", nullable: true),
                    range_end = table.Column<DateOnly>(type: "date", nullable: true),
                    queued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    symbols_requested = table.Column<int>(type: "integer", nullable: false),
                    symbols_received = table.Column<int>(type: "integer", nullable: false),
                    records_inserted = table.Column<int>(type: "integer", nullable: false),
                    records_skipped = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingestion_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_ingestion_runs_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "industries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sector_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_industries", x => x.id);
                    table.ForeignKey(
                        name: "fk_industries_sectors_sector_id",
                        column: x => x.sector_id,
                        principalTable: "sectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_logins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    linked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_logins", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolios", x => x.id);
                    table.ForeignKey(
                        name: "fk_portfolios_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    sector_id = table.Column<int>(type: "integer", nullable: true),
                    industry_id = table.Column<int>(type: "integer", nullable: true),
                    cik = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    homepage_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    employee_count = table.Column<int>(type: "integer", nullable: true),
                    listed_on = table.Column<DateOnly>(type: "date", nullable: true),
                    profile_refreshed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_companies_industries_industry_id",
                        column: x => x.industry_id,
                        principalTable: "industries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_companies_sectors_sector_id",
                        column: x => x.sector_id,
                        principalTable: "sectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    symbol = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    exchange = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    asset_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_tracked = table.Column<bool>(type: "boolean", nullable: false),
                    tracked_since = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    untracked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delisted_on = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_stocks_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "daily_prices",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stock_id = table.Column<int>(type: "integer", nullable: false),
                    trade_date = table.Column<DateOnly>(type: "date", nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    high = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    low = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    close = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    volume = table.Column<long>(type: "bigint", nullable: false),
                    volume_weighted_average_price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    transaction_count = table.Column<int>(type: "integer", nullable: true),
                    data_source_id = table.Column<int>(type: "integer", nullable: false),
                    ingested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_prices", x => x.id);
                    table.CheckConstraint("ck_daily_prices_high_low", "high >= low");
                    table.CheckConstraint("ck_daily_prices_positive", "open > 0 AND high > 0 AND low > 0 AND close > 0");
                    table.CheckConstraint("ck_daily_prices_volume", "volume >= 0");
                    table.ForeignKey(
                        name: "fk_daily_prices_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_daily_prices_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ml_predictions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    stock_id = table.Column<int>(type: "integer", nullable: false),
                    prediction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    target_date = table.Column<DateOnly>(type: "date", nullable: false),
                    horizon_days = table.Column<int>(type: "integer", nullable: false),
                    predicted_close = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    predicted_return = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    direction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    signal = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(9,8)", precision: 9, scale: 8, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ml_predictions", x => x.id);
                    table.CheckConstraint("ck_ml_predictions_confidence", "confidence IS NULL OR (confidence >= 0 AND confidence <= 1)");
                    table.ForeignKey(
                        name: "fk_ml_predictions_ml_models_model_id",
                        column: x => x.model_id,
                        principalTable: "ml_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ml_predictions_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_holdings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    average_cost = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    opened_on = table.Column<DateOnly>(type: "date", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_holdings", x => x.id);
                    table.CheckConstraint("ck_portfolio_holdings_average_cost", "average_cost >= 0");
                    table.CheckConstraint("ck_portfolio_holdings_quantity", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_portfolio_holdings_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalTable: "portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portfolio_holdings_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ml_prediction_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prediction_id = table.Column<long>(type: "bigint", nullable: false),
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    stock_id = table.Column<int>(type: "integer", nullable: false),
                    target_date = table.Column<DateOnly>(type: "date", nullable: false),
                    predicted_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    actual_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    absolute_error = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    percentage_error = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    direction_correct = table.Column<bool>(type: "boolean", nullable: true),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ml_prediction_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_ml_prediction_history_ml_models_model_id",
                        column: x => x.model_id,
                        principalTable: "ml_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ml_prediction_history_ml_predictions_prediction_id",
                        column: x => x.prediction_id,
                        principalTable: "ml_predictions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ml_prediction_history_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "data_sources",
                columns: new[] { "id", "key", "name" },
                values: new object[,]
                {
                    { 1, "polygon", "Polygon.io" },
                    { 2, "mock", "Deterministic Mock Provider" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "name", "normalized_name" },
                values: new object[,]
                {
                    { 1, "Full access, including internal ingestion endpoints and universe management.", "Administrator", "ADMINISTRATOR" },
                    { 2, "Standard access to market data, portfolios and recommendations.", "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "sectors",
                columns: new[] { "id", "display_order", "key", "name" },
                values: new object[,]
                {
                    { 1, 1, "Energy", "Energy" },
                    { 2, 2, "Materials", "Materials" },
                    { 3, 3, "Industrials", "Industrials" },
                    { 4, 4, "ConsumerDiscretionary", "Consumer Discretionary" },
                    { 5, 5, "ConsumerStaples", "Consumer Staples" },
                    { 6, 6, "HealthCare", "Health Care" },
                    { 7, 7, "Financials", "Financials" },
                    { 8, 8, "InformationTechnology", "Information Technology" },
                    { 9, 9, "CommunicationServices", "Communication Services" },
                    { 10, 10, "Utilities", "Utilities" },
                    { 11, 11, "RealEstate", "Real Estate" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_companies_industry_id",
                table: "companies",
                column: "industry_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_name",
                table: "companies",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_companies_sector_id",
                table: "companies",
                column: "sector_id");

            migrationBuilder.CreateIndex(
                name: "ix_daily_prices_data_source_id",
                table: "daily_prices",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_daily_prices_trade_date_brin",
                table: "daily_prices",
                column: "trade_date")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "ux_daily_prices_stock_trade_date",
                table: "daily_prices",
                columns: new[] { "stock_id", "trade_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_sources_key",
                table: "data_sources",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_logins_provider_provider_key",
                table: "external_logins",
                columns: new[] { "provider", "provider_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_logins_user_id",
                table: "external_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_industries_sector_id_name",
                table: "industries",
                columns: new[] { "sector_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_runs_data_source_id",
                table: "ingestion_runs",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_runs_queued_at",
                table: "ingestion_runs",
                column: "queued_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_runs_run_type_trade_date_status",
                table: "ingestion_runs",
                columns: new[] { "run_type", "trade_date", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_ml_models_key_version",
                table: "ml_models",
                columns: new[] { "key", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ml_prediction_history_model_id_target_date",
                table: "ml_prediction_history",
                columns: new[] { "model_id", "target_date" });

            migrationBuilder.CreateIndex(
                name: "ix_ml_prediction_history_prediction_id",
                table: "ml_prediction_history",
                column: "prediction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ml_prediction_history_stock_id",
                table: "ml_prediction_history",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "ix_ml_predictions_stock_id_prediction_date",
                table: "ml_predictions",
                columns: new[] { "stock_id", "prediction_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ux_ml_predictions_model_stock_dates",
                table: "ml_predictions",
                columns: new[] { "model_id", "stock_id", "prediction_date", "target_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_holdings_portfolio_id_stock_id",
                table: "portfolio_holdings",
                columns: new[] { "portfolio_id", "stock_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_holdings_stock_id",
                table: "portfolio_holdings",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "ix_portfolios_user_id",
                table: "portfolios",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_portfolios_user_id_name",
                table: "portfolios",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_expires_at",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_roles_normalized_name",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sectors_key",
                table: "sectors",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stocks_company_id",
                table: "stocks",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_is_tracked",
                table: "stocks",
                column: "is_tracked",
                filter: "is_tracked");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_symbol",
                table: "stocks",
                column: "symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_prices");

            migrationBuilder.DropTable(
                name: "external_logins");

            migrationBuilder.DropTable(
                name: "ingestion_runs");

            migrationBuilder.DropTable(
                name: "ml_prediction_history");

            migrationBuilder.DropTable(
                name: "portfolio_holdings");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "data_sources");

            migrationBuilder.DropTable(
                name: "ml_predictions");

            migrationBuilder.DropTable(
                name: "portfolios");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "ml_models");

            migrationBuilder.DropTable(
                name: "stocks");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "industries");

            migrationBuilder.DropTable(
                name: "sectors");
        }
    }
}
