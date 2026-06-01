--liquibase formatted sql

--changeset repo-admin:baseline-cross-schema labels:baseline context:all
-- Cross-schema views (requires both fps and mabarchive schemas)
-- Generated from pg_dump and cleaned for Liquibase

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;
--
-- Name: vfps_totals; Type: VIEW; Schema: fps; Owner: -
--

CREATE VIEW "fps"."vfps_totals" AS
 SELECT "fpsyeartotals"."parentproject",
    "fpsyeartotals"."program",
    "fpsyeartotals"."totaladditionalcosts",
    "fpsyeartotals"."totalanimalcosts",
    "fpsyeartotals"."totalstaffcosts",
    "fpsyeartotals"."totaltestcosts",
    "fpsyeartotals"."totalcosts",
    "fpsyeartotals"."custincome",
    "fpsyeartotals"."transferincome",
    "fpsyeartotals"."totalincome",
    "fpsyeartotals"."budget_cvl",
    "fpsyeartotals"."requiredprofit",
    "fpsyeartotals"."manager",
    "fpsyeartotals"."customer",
    "fpsyeartotals"."projectstatus",
    "fpsyeartotals"."pvsincome",
    "fpsyeartotals"."plancaseworkdebit",
    "ma_a"."bfbudget" AS "ma_bfbudget",
    "fpsyeartotals"."fpsyear"
   FROM ("fps"."fpsyeartotals"
     LEFT JOIN ( SELECT "my_tlkpprojectradtrackdata"."project",
            "my_tlkpprojectradtrackdata"."bfbudget"
           FROM "mabarchive"."my_tlkpprojectradtrackdata"
          WHERE (("my_tlkpprojectradtrackdata"."year")::"text" = ( SELECT "right"(("tbldb_variables"."db_var_value")::"text", 4) AS "right"
                   FROM "fps"."tbldb_variables"
                  WHERE (("tbldb_variables"."db_var_name")::"text" = 'DB_Name'::"text")))) "ma_a" ON ((("fpsyeartotals"."parentproject")::"text" = ("ma_a"."project")::"text")));





--rollback empty
