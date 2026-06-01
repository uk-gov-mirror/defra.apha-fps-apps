--liquibase formatted sql

--changeset repo-admin:baseline-mabarchive labels:baseline context:all
-- Baseline schema and table creation for mabarchive schema
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
CREATE SCHEMA "mabarchive";

SET default_tablespace = '';

SET default_table_access_method = "heap";
--
-- Name: my_tlkpprojectradtrackdata; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tlkpprojectradtrackdata" (
    "year" smallint NOT NULL,
    "project" character varying(20) NOT NULL,
    "bfbudget" "money",
    "pybudget" "money",
    "seedcorn" "money",
    "manhours" double precision,
    "mandays" double precision,
    "manyears" double precision,
    "paycosts" "money",
    "nonpayohcosts" "money",
    "testcosts" "money",
    "animalcosts" "money",
    "nonanimalcosts" "money",
    "manhourschanged" smallint DEFAULT 0,
    "paycostschanged" smallint DEFAULT 0,
    "nonpayohcostschanged" smallint DEFAULT 0,
    "testcostschanged" smallint DEFAULT 0,
    "animalcostschanged" smallint DEFAULT 0,
    "nonanimalcostschanged" smallint DEFAULT 0,
    "adjustment" "money",
    "adjustmentcomment" character varying(250),
    "locked" smallint DEFAULT 0,
    "datecosted" timestamp without time zone,
    "costedby" character varying(20),
    "actualexpenditure" "money",
    "actualmanyears" double precision,
    "vla_budget" "money"
);


--
-- Name: _tmpupdatestatus; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."_tmpupdatestatus" (
    "parentproject" character varying(20) NOT NULL,
    "fpsstatus" character varying(50),
    "mastatus" character varying(50),
    "year" smallint
);


--
-- Name: g_tlkpproject; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."g_tlkpproject" (
    "parentproject" character varying(20) NOT NULL,
    "projecttitle" character varying(200),
    "costbookno" character varying(50),
    "disease" character varying(50),
    "contract" character varying(10),
    "shorttitle" character varying(30),
    "projectstatus" character varying(50)
);


--
-- Name: g_tlkpproject_radtrackdata; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."g_tlkpproject_radtrackdata" (
    "parentproject" character varying(20) NOT NULL,
    "version" character varying(10),
    "fileref" character varying(20),
    "customerref" character varying(20),
    "startdate" timestamp without time zone,
    "enddate" timestamp without time zone,
    "finalreportreceived" timestamp without time zone,
    "finalreportsent" timestamp without time zone,
    "inflation" smallint DEFAULT 0,
    "closeddate" timestamp without time zone,
    "useprojectyear" smallint DEFAULT 0 NOT NULL,
    "status" character varying(50),
    "pcforecastspend" double precision,
    "riskid" integer,
    "costbooknumber" character varying(10),
    "revisedenddate" timestamp without time zone,
    "formrequired" boolean DEFAULT true NOT NULL,
    "overallcustincome" "money"
);


--
-- Name: my_fpsyeartotals; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_fpsyeartotals" (
    "year" smallint NOT NULL,
    "parentproject" character varying(20) NOT NULL,
    "program" character varying(10) NOT NULL,
    "totaladditionalcosts" "money",
    "totalanimalcosts" double precision,
    "totalstaffcosts" double precision,
    "totaltestcosts" double precision,
    "totalcosts" double precision,
    "custincome" "money" NOT NULL,
    "transferincome" "money" NOT NULL,
    "totalincome" "money" NOT NULL,
    "budget_cvl" "money",
    "requiredprofit" "money",
    "manager" character varying(50),
    "customer" character varying(50),
    "projectstatus" character varying(50) NOT NULL,
    "pvsincome" "money",
    "plancaseworkdebit" "money",
    "totalpaycosts" double precision
);


--
-- Name: my_milestoneformdates; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_milestoneformdates" (
    "year" smallint NOT NULL,
    "parentproject" character varying(20) NOT NULL,
    "jan" timestamp without time zone,
    "feb" timestamp without time zone,
    "mar" timestamp without time zone,
    "apr" timestamp without time zone,
    "may" timestamp without time zone,
    "jun" timestamp without time zone,
    "jul" timestamp without time zone,
    "aug" timestamp without time zone,
    "sep" timestamp without time zone,
    "oct" timestamp without time zone,
    "nov" timestamp without time zone,
    "dec" timestamp without time zone
);


--
-- Name: my_monthlyoutput; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_monthlyoutput" (
    "year" smallint NOT NULL,
    "testcode" character varying(20) NOT NULL,
    "buyer" character varying(20) NOT NULL,
    "month" double precision NOT NULL,
    "workgroup" character varying(50) NOT NULL,
    "volume" double precision,
    "wgbuyer" character varying(50)
);


--
-- Name: my_monthlytime; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_monthlytime" (
    "year" smallint NOT NULL,
    "pactstaffid" character varying(50) NOT NULL,
    "timecode" character varying(50) NOT NULL,
    "month" double precision NOT NULL,
    "parentproject" character varying(20) NOT NULL,
    "workgroup" character varying(50),
    "hours" double precision
);


--
-- Name: my_profitcentregrade; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_profitcentregrade" (
    "year" integer NOT NULL,
    "pcgrade" character varying(20) NOT NULL,
    "divisiongrade" character varying(10) NOT NULL,
    "gradecode" character varying(10) NOT NULL,
    "profitcentre" character varying(50) NOT NULL,
    "chargerate" "money",
    "directrate" "money",
    "payrate" "money",
    "npr" "money",
    "ohr" "money"
);


--
-- Name: my_proj_invoice; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_proj_invoice" (
    "year" smallint NOT NULL,
    "projectparent" character varying(20) NOT NULL,
    "month" integer,
    "amount" "money",
    "costofwork" "money",
    "wip" "money",
    "profitloss" "money",
    "detail" character varying(100),
    "invoicecounter" integer NOT NULL,
    "type" character varying(10)
);


--
-- Name: my_proj_subcontract; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_proj_subcontract" (
    "year" smallint NOT NULL,
    "subcontcounter" integer NOT NULL,
    "project" character varying(20),
    "testjob" character varying(50),
    "month" double precision,
    "amount" "money",
    "workgroup" character varying(50),
    "acctcode" character varying(30),
    "supplier" character varying(50),
    "description" character varying(255),
    "suppliernumber" integer,
    "dailyrate" "money",
    "animaldays" integer
);


--
-- Name: my_projectmonthfinal; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_projectmonthfinal" (
    "year" smallint NOT NULL,
    "project" character varying(20) NOT NULL,
    "monthno" double precision NOT NULL,
    "periodname" character varying(50),
    "cumflag" double precision,
    "costprofile" "money",
    "subcontracts" "money",
    "animals" "money",
    "nonanimals" "money",
    "timecosts" "money",
    "transfercosts" "money",
    "totalcost" "money",
    "invoices" "money",
    "coiw" "money",
    "portsales" "money",
    "cumcost" "money",
    "cumprofile" "money",
    "sumofcostprofile" "money",
    "cuminvoices" "money",
    "cumcoiw" "money",
    "cumportsales" "money",
    "mstonedue" integer,
    "due__done" double precision,
    "ontime" double precision,
    "sumofmstonedue" double precision,
    "sumofdue__done" double precision,
    "sumofontime" double precision,
    "cwdebit" "money",
    "cwcredit" "money",
    "cumcwdebit" "money",
    "cumcwcredit" "money",
    "totalhours" double precision,
    "cumtotalhours" double precision,
    "cumsubcontracts" double precision,
    "cumtestcosts" double precision,
    "paycosts" double precision,
    "cumpaycosts" double precision
);


--
-- Name: my_radtrack_reports; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_radtrack_reports" (
    "year" smallint NOT NULL,
    "project" character varying(20) NOT NULL,
    "type" character varying(10) NOT NULL,
    "reminder1" timestamp without time zone,
    "reminder2" timestamp without time zone,
    "replyreceived" timestamp without time zone,
    "senttoprogmanager" timestamp without time zone,
    "senttoprojleader" timestamp without time zone,
    "emailedtocustomer" timestamp without time zone,
    "signedcopytocustomer" timestamp without time zone,
    "repduedate" timestamp without time zone,
    "id" integer NOT NULL,
    "reportagreeddate" timestamp without time zone
);


--
-- Name: my_radtrack_reports_id_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."my_radtrack_reports" ALTER COLUMN "id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."my_radtrack_reports_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: my_staff; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_staff" (
    "year" smallint NOT NULL,
    "staffid" character varying(50) NOT NULL,
    "workgroupgrade" character varying(50) NOT NULL,
    "name" character varying(50) NOT NULL,
    "title" character varying(4),
    "personstatus" character varying(10),
    "personclass" character varying(10),
    "hrspaid" double precision,
    "leave" double precision,
    "sickspecial" double precision,
    "hrsavail" double precision
);


--
-- Name: my_tbladditionalcosts; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tbladditionalcosts" (
    "year" smallint NOT NULL,
    "jobcode" character varying(20) NOT NULL,
    "account" character varying(50) NOT NULL,
    "description" character varying(20) NOT NULL,
    "itemcost" "money" NOT NULL,
    "freq" character varying(5),
    "supplier" character varying(50),
    "ac_counter" integer NOT NULL
);


--
-- Name: my_tbladditionalcosts_ac_counter_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."my_tbladditionalcosts" ALTER COLUMN "ac_counter" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."my_tbladditionalcosts_ac_counter_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: my_tblanimalreq; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tblanimalreq" (
    "year" smallint NOT NULL,
    "jobcode" character varying(20) NOT NULL,
    "animaltype" character varying(50) NOT NULL,
    "numberofdays" double precision NOT NULL,
    "numberofanimals" double precision NOT NULL,
    "ar_counter" integer NOT NULL
);


--
-- Name: my_tblanimalreq_ar_counter_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."my_tblanimalreq" ALTER COLUMN "ar_counter" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."my_tblanimalreq_ar_counter_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: my_tblanimals; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tblanimals" (
    "year" smallint NOT NULL,
    "animaltype" character varying(50) NOT NULL,
    "species" character varying(50),
    "security_level" character varying(50),
    "dailyrate" "money",
    "planbyweek" boolean,
    "defradailyrate" "money"
);


--
-- Name: my_tblcontract; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tblcontract" (
    "year" smallint NOT NULL,
    "contractno" character varying(10) NOT NULL,
    "category" character varying(20) NOT NULL,
    "manager" character varying(50),
    "customer" character varying(50),
    "title" character varying(100),
    "registereddate" timestamp without time zone,
    "startdate" timestamp without time zone,
    "enddate" timestamp without time zone,
    "contractdoc" "bytea",
    "duration" integer
);


--
-- Name: my_tblprofitcentre; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tblprofitcentre" (
    "year" smallint NOT NULL,
    "profitcentre" character varying(50) NOT NULL,
    "profitcentrename" character varying(40) NOT NULL,
    "division" character varying(10) NOT NULL,
    "conttarget" "money",
    "profitcentrehead" character varying(50),
    "divisionid" integer
);


--
-- Name: my_tblstaffjob; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tblstaffjob" (
    "year" smallint NOT NULL,
    "staffid" character varying(50) NOT NULL,
    "jobcode" character varying(20) NOT NULL,
    "plannedhours" double precision NOT NULL,
    "dummy_col" character varying(10)
);


--
-- Name: my_testorproduct; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_testorproduct" (
    "year" smallint NOT NULL,
    "itemcode" character varying(20) NOT NULL,
    "itemdescription" character varying(200),
    "testmanager" character varying(50),
    "jobstatus" character varying(2),
    "unitpricevla" "money",
    "priceahvg" "money",
    "owner" character varying(2),
    "chargemethod" character varying(5),
    "shortdescription" character(18),
    "defraunitprice" "money"
);


--
-- Name: my_timecostcalcs; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_timecostcalcs" (
    "year" smallint NOT NULL,
    "workgroup" character varying(50) NOT NULL,
    "jobcode" character varying(50) NOT NULL,
    "project" character varying(20) NOT NULL,
    "month" double precision NOT NULL,
    "staffid" character varying(50) NOT NULL,
    "gradecode" character varying(10),
    "name" character varying(50),
    "chargerate" "money",
    "class" character varying(255),
    "time" double precision,
    "cost" double precision,
    "division" character varying(10),
    "jobcodeold" character varying(14),
    "pay" "money",
    "nonpay" "money",
    "overhead" "money"
);


--
-- Name: my_tlkpprogram; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tlkpprogram" (
    "year" smallint NOT NULL,
    "programno" character varying(10) NOT NULL,
    "programname" character varying(80),
    "directorate" character varying(15),
    "minim" character varying(7),
    "sector_name" character varying(50),
    "customer" character varying(50),
    "target" "money",
    "manager" character varying(50)
);


--
-- Name: my_tlkpproject; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tlkpproject" (
    "year" smallint NOT NULL,
    "parentproject" character varying(20) NOT NULL,
    "program" character varying(10),
    "customer" character varying(50),
    "manager" character varying(50),
    "transferincome" "money",
    "custincome" "money",
    "wip_eoy" "money",
    "wip_limit" "money",
    "wip_current" "money",
    "projectstatus" character varying(50),
    "datecreated" timestamp without time zone,
    "feccost" "money",
    "profit" "money",
    "budget_cvl" "money",
    "caseworksub" numeric(5,4),
    "pvsincome" "money",
    "plancaseworkdebit" "money",
    "source" character(5),
    "disease" character varying(50),
    "contract" character varying(10),
    "finished" smallint,
    "comments" "text",
    "carryover" "money",
    "isdefraproject" smallint,
    "costcentre" double precision,
    "oracleprojectcode" character varying(50),
    "subaccountcode" character varying(50),
    "projectgroup" character varying(50),
    "incomeaccountcode" character varying(50)
);


--
-- Name: my_tlkpproject_all; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tlkpproject_all" (
    "year" smallint NOT NULL,
    "parentproject" character varying(20) NOT NULL,
    "program" character varying(10),
    "customer" character varying(50),
    "manager" character varying(50),
    "transferincome" "money",
    "custincome" "money",
    "wip_eoy" "money",
    "wip_limit" "money",
    "wip_current" "money",
    "projectstatus" character varying(50),
    "datecreated" timestamp without time zone,
    "feccost" "money",
    "profit" "money",
    "budget_cvl" "money",
    "caseworksub" numeric(5,4),
    "pvsincome" "money",
    "plancaseworkdebit" "money",
    "source" character(5),
    "disease" character varying(50),
    "contract" character varying(10),
    "finished" smallint,
    "comments" "text",
    "carryover" "money",
    "isdefraproject" smallint,
    "costcentre" double precision,
    "oracleprojectcode" character varying(50),
    "subaccountcode" character varying(50),
    "projectgroup" character varying(50),
    "incomeaccountcode" character varying(50)
);


--
-- Name: my_tlkptestreqmt; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_tlkptestreqmt" (
    "year" smallint NOT NULL,
    "testcode" character varying(20) NOT NULL,
    "buyer" character varying(20) NOT NULL,
    "unitprice" "money",
    "norequired" double precision,
    "projectbuyercode" character varying(50),
    "testbuyercode" character varying(50),
    "source" character(5)
);


--
-- Name: my_workgroup; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_workgroup" (
    "year" smallint NOT NULL,
    "workgroup" character varying(50) NOT NULL,
    "profitcentre" character varying(50) NOT NULL,
    "costcentre" double precision,
    "owner" character varying(50),
    "description" character varying(45),
    "centraloverhead" "money",
    "sendemail" smallint,
    "cos90" smallint,
    "costcentreold" double precision,
    "email_recipient" character varying(50)
);


--
-- Name: my_workgroupgrade; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."my_workgroupgrade" (
    "year" integer NOT NULL,
    "wggrade" character varying(50) NOT NULL,
    "profitcentregrade" character varying(20) NOT NULL,
    "gradecode" character varying(10) NOT NULL,
    "workgroup" character varying(50) NOT NULL
);


--
-- Name: tbl_settings; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbl_settings" (
    "id" character varying(50) NOT NULL,
    "setting" character varying(255),
    "notes" character varying(255),
    "testsetting" character varying(255),
    "userupdateable" boolean DEFAULT false
);


--
-- Name: tblaccesslevels; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblaccesslevels" (
    "systemid" integer NOT NULL,
    "accesslevelid" integer NOT NULL,
    "accesslevel" character varying(50)
);


--
-- Name: tblaccessprograms; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblaccessprograms" (
    "systemid" integer NOT NULL,
    "ntlogin" character varying(50) NOT NULL,
    "program" character varying(10) NOT NULL
);


--
-- Name: tblaccesssystems; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblaccesssystems" (
    "systemid" integer NOT NULL,
    "systemname" character varying(50) NOT NULL
);


--
-- Name: tblaccessusers; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblaccessusers" (
    "systemid" integer NOT NULL,
    "ntlogin" character varying(50) NOT NULL,
    "username" character varying(50),
    "dt2login" character varying(50)
);


--
-- Name: tblaccessusers_levels; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblaccessusers_levels" (
    "systemid" integer NOT NULL,
    "ntlogin" character varying(50) NOT NULL,
    "accesslevelid" integer NOT NULL
);


--
-- Name: tbladditionalcosts; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbladditionalcosts" (
    "ac_identity" integer NOT NULL,
    "project" character varying(50),
    "year" integer DEFAULT 0,
    "accountcat" character varying(50) NOT NULL,
    "description" character varying(100) NOT NULL,
    "itemcost" double precision DEFAULT 0,
    "costentered" double precision DEFAULT 0 NOT NULL,
    "freq" character varying(5)
);


--
-- Name: tbladditionalcosts_ac_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tbladditionalcosts" ALTER COLUMN "ac_identity" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tbladditionalcosts_ac_identity_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblanimalreq; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblanimalreq" (
    "ar_identity" integer NOT NULL,
    "project" character varying(50),
    "year" integer DEFAULT 0,
    "animaltype" character varying(50) NOT NULL,
    "number_of_days" double precision DEFAULT 0,
    "number_of_animals" double precision DEFAULT 0,
    "dailyrate" double precision DEFAULT 0
);


--
-- Name: tblanimalreq_ar_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblanimalreq" ALTER COLUMN "ar_identity" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblanimalreq_ar_identity_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblcapsstaff; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblcapsstaff" (
    "mnumber" character varying(50) NOT NULL,
    "name" character varying(50) NOT NULL,
    "dt2number" character varying(50)
);


--
-- Name: tblcomments; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblcomments" (
    "commentno" integer NOT NULL,
    "project" character varying(20) NOT NULL,
    "year" smallint NOT NULL,
    "dateentered" timestamp without time zone,
    "topic" character varying(25) NOT NULL,
    "comment" character varying,
    "madeby" character varying(50)
);


--
-- Name: tblcomments_commentno_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblcomments" ALTER COLUMN "commentno" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblcomments_commentno_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblcsg7_accountgroups; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblcsg7_accountgroups" (
    "csg7group" character varying(15) NOT NULL,
    "useinflation" boolean DEFAULT true
);


--
-- Name: tbldb_var; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbldb_var" (
    "year" integer
);


--
-- Name: tbldbvariables; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbldbvariables" (
    "db_variable" character varying(50) NOT NULL,
    "nval" double precision DEFAULT 0
);


--
-- Name: tbldisease; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbldisease" (
    "disease" character varying(50) NOT NULL
);


--
-- Name: tbleugrade_conversion; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbleugrade_conversion" (
    "vlagrade" character varying(50) NOT NULL,
    "eugrade" character varying(50)
);


--
-- Name: tblfpsyearstoimport; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblfpsyearstoimport" (
    "fpsname" character varying(10) NOT NULL
);


--
-- Name: tblimages; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblimages" (
    "imageid" integer NOT NULL,
    "image" "bytea",
    "decription" character varying(50)
);


--
-- Name: tbllogmilestone; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbllogmilestone" (
    "project" character varying(20),
    "number" character varying(10),
    "description" character varying(500),
    "datedue" timestamp without time zone,
    "datecompleted" timestamp without time zone,
    "dateformreceived" timestamp without time zone,
    "undersdreview" smallint,
    "ontarget" smallint,
    "projectleadercomment" character varying,
    "capscomment" character varying(250),
    "idtype" character(1),
    "datechanged" timestamp without time zone,
    "changedby" character varying(10),
    "updatetype" character(1),
    "id" integer NOT NULL
);


--
-- Name: tbllogmilestone_id_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tbllogmilestone" ALTER COLUMN "id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tbllogmilestone_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblmaintenance; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblmaintenance" (
    "formname" character varying(50) NOT NULL,
    "description" character varying(50),
    "usernotes" character varying(250),
    "is_obsolete" boolean NOT NULL,
    "displayseq" integer
);


--
-- Name: tblmilestone; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblmilestone" (
    "project" character varying(20) NOT NULL,
    "number" character varying(10) NOT NULL,
    "description" character varying(500),
    "datedue" timestamp without time zone NOT NULL,
    "datecompleted" timestamp without time zone,
    "dateformreceived" timestamp without time zone,
    "undersdreview" smallint DEFAULT 0,
    "ontarget" smallint DEFAULT 0,
    "projectleadercomment" character varying,
    "capscomment" character varying(250),
    "idtype" character(1)
);


--
-- Name: tblprofitcentre_manager_link; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblprofitcentre_manager_link" (
    "profitcentre" character varying(50) NOT NULL,
    "manager" character varying(50) NOT NULL
);


--
-- Name: tblprogram_manager_link; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblprogram_manager_link" (
    "program" character varying(50) NOT NULL,
    "manager" character varying(50) NOT NULL
);


--
-- Name: tblproject; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblproject" (
    "project" character varying(50) NOT NULL,
    "plancat" character varying(50),
    "projecttitle" character varying(100),
    "programme" character varying(50),
    "projectworkgroup" character varying(50),
    "contractprice" double precision,
    "startdate" timestamp without time zone,
    "disease" character varying(50),
    "startfyear" double precision DEFAULT 0,
    "customer_name" character varying(50),
    "contract_number" character varying(50),
    "submittedbyfname" character varying(50),
    "submittedbylname" character varying(50),
    "date_of_submission" timestamp without time zone,
    "prepared_by" character varying(50),
    "inflation" integer DEFAULT 0,
    "financialyears" integer,
    "notes" character varying(255),
    "euroconvrate" double precision,
    "isdefraproject" smallint
);


--
-- Name: tblprojectmanager; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblprojectmanager" (
    "projectmanager" character varying(50) NOT NULL,
    "email" character varying(255),
    "mnumber" character varying(10),
    "disable" boolean DEFAULT false NOT NULL
);


--
-- Name: tblprojectreviewitems; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblprojectreviewitems" (
    "project" character varying(50) NOT NULL,
    "itemid" integer NOT NULL,
    "frequencyid" integer
);


--
-- Name: tblprojectyear; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblprojectyear" (
    "project" character varying(50) NOT NULL,
    "yearno" integer NOT NULL,
    "markup_time" double precision,
    "markup_tests" double precision,
    "markup_animals" double precision,
    "markup_additional" double precision,
    "profit_time" double precision,
    "profit_tests" double precision,
    "profit_animals" double precision,
    "profit_additional" double precision
);


--
-- Name: tblproposedproject; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblproposedproject" (
    "id" integer NOT NULL,
    "parentproject" character varying(20) NOT NULL,
    "projecttitle" character varying(200),
    "program" character varying(10),
    "customer" character varying(50),
    "manager" character varying(50),
    "projectstatus" character varying(50),
    "costbookno" character varying(50),
    "disease" character varying(50),
    "reason" character varying(250)
);


--
-- Name: tblproposedproject_id_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblproposedproject" ALTER COLUMN "id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblproposedproject_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblpublication; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblpublication" (
    "uid" integer NOT NULL,
    "identifier" character varying(50) NOT NULL,
    "type" character varying(3) NOT NULL,
    "program" character varying(10) NOT NULL,
    "subject" character varying(500),
    "leadauthor" character varying(50),
    "otherauthors" character varying(255),
    "targetdate" timestamp without time zone,
    "submitted" timestamp without time zone,
    "published" boolean NOT NULL,
    "comments" "text"
);


--
-- Name: tblpublication_uid_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblpublication" ALTER COLUMN "uid" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblpublication_uid_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblpublicationproject; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblpublicationproject" (
    "publicationuid" integer NOT NULL,
    "parentproject" character varying(20) NOT NULL
);


--
-- Name: tblradtrackcontract; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblradtrackcontract" (
    "contract" character varying(10) NOT NULL
);


--
-- Name: tblradtrackinvoice; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblradtrackinvoice" (
    "invoicecounter" integer NOT NULL,
    "project" character varying(20),
    "plannedamount" double precision,
    "dueamount" double precision,
    "duedate" timestamp without time zone,
    "actualamount" double precision,
    "dateinvoiced" timestamp without time zone,
    "contract" character varying(10),
    "datejobsheetraised" timestamp without time zone,
    "invoiceref" character varying(50),
    "invoicepaid" smallint DEFAULT 0 NOT NULL
);


--
-- Name: tblradtrackinvoice_invoicecounter_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblradtrackinvoice" ALTER COLUMN "invoicecounter" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblradtrackinvoice_invoicecounter_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblradtrackprog; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblradtrackprog" (
    "program" character varying(10) NOT NULL,
    "radtrackprog" boolean DEFAULT true NOT NULL,
    "publicationprefix" character varying(5)
);


--
-- Name: tblreport; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblreport" (
    "id" integer NOT NULL,
    "reportname" character varying(50) NOT NULL,
    "reportdescription" character varying(50),
    "filter" character varying(200),
    "mailcomment" character varying(250),
    "mailtitle" character varying(50),
    "emailable" boolean NOT NULL,
    "sortorder" integer,
    "allowpickprogramme" boolean NOT NULL,
    "allowpickproject" boolean NOT NULL,
    "allowpickmanager" boolean NOT NULL,
    "allowpickcontract" boolean NOT NULL,
    "allowpickcustomer" boolean NOT NULL,
    "allowpickmonth" boolean NOT NULL,
    "allowpickfyear" boolean NOT NULL,
    "reporthelp" character varying(250),
    "type" character(1) NOT NULL
);


--
-- Name: tblreportgroup; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblreportgroup" (
    "groupid" integer NOT NULL,
    "description" character varying(50) NOT NULL
);


--
-- Name: tblreportgroup_groupid_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblreportgroup" ALTER COLUMN "groupid" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblreportgroup_groupid_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tblreportgroup_link; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblreportgroup_link" (
    "reportid" integer NOT NULL,
    "groupid" integer NOT NULL
);


--
-- Name: tblstaffrequ; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tblstaffrequ" (
    "sr_identity" integer NOT NULL,
    "project" character varying(50),
    "year" integer DEFAULT 0,
    "wggrade" character varying(20) NOT NULL,
    "name" character varying(50),
    "nohours" double precision DEFAULT 0,
    "nodays" double precision DEFAULT 0,
    "chargerate" double precision DEFAULT 0,
    "payrate" double precision,
    "npr" double precision,
    "ohr" double precision
);


--
-- Name: tblstaffrequ_sr_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."tblstaffrequ" ALTER COLUMN "sr_identity" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."tblstaffrequ_sr_identity_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tbltestrequ; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tbltestrequ" (
    "project" character varying(50) NOT NULL,
    "year" integer DEFAULT 0 NOT NULL,
    "testcode" character varying(50) NOT NULL,
    "notests" double precision DEFAULT 0,
    "unitprice" double precision DEFAULT 0
);


--
-- Name: temptbladditionalcosts; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."temptbladditionalcosts" (
    "ac_identity" integer NOT NULL,
    "project" integer DEFAULT 0,
    "year" integer DEFAULT 0,
    "accountcat" character varying(50),
    "description" character varying(20),
    "itemcost" double precision DEFAULT 0,
    "costentered" double precision DEFAULT 0,
    "freq" character varying(5)
);


--
-- Name: temptbladditionalcosts_ac_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."temptbladditionalcosts" ALTER COLUMN "ac_identity" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."temptbladditionalcosts_ac_identity_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: temptblanimalreq; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."temptblanimalreq" (
    "ar_identity" integer NOT NULL,
    "project" integer DEFAULT 0,
    "year" integer DEFAULT 0,
    "animaltype" character varying(50),
    "number_of_days" double precision DEFAULT 0,
    "number_of_animals" double precision DEFAULT 0,
    "dailyrate" double precision DEFAULT 0
);


--
-- Name: temptblanimalreq_ar_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."temptblanimalreq" ALTER COLUMN "ar_identity" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."temptblanimalreq_ar_identity_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: temptblproject; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."temptblproject" (
    "project" integer DEFAULT 0 NOT NULL,
    "programme" character varying(10),
    "plancat" character varying(50),
    "projecttitle" character varying(100),
    "projectworkgroup" character varying(50),
    "contractprice" double precision,
    "startdate" timestamp without time zone,
    "disease" character varying(50),
    "startfyear" double precision DEFAULT 0,
    "customer_name" character varying(50),
    "contract_number" character varying(50),
    "submitted_by" character varying(50),
    "date_of_submission" timestamp without time zone,
    "prepared_by" character varying(50),
    "inflation" boolean DEFAULT false,
    "ready" boolean DEFAULT false,
    "financialyears" boolean DEFAULT true,
    "notes" character varying(1000)
);


--
-- Name: temptblprojectyear; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."temptblprojectyear" (
    "project" integer DEFAULT 0 NOT NULL,
    "yearno" integer NOT NULL
);


--
-- Name: temptblstaffrequ; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."temptblstaffrequ" (
    "sr_identity" integer NOT NULL,
    "project" integer DEFAULT 0,
    "year" integer DEFAULT 0,
    "wggrade" character varying(20),
    "name" character varying(50),
    "nohours" double precision DEFAULT 0,
    "nodays" double precision DEFAULT 0,
    "chargerate" double precision DEFAULT 0
);


--
-- Name: temptblstaffrequ_sr_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
--

ALTER TABLE "mabarchive"."temptblstaffrequ" ALTER COLUMN "sr_identity" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "mabarchive"."temptblstaffrequ_sr_identity_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: temptbltestreq; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."temptbltestreq" (
    "project" integer DEFAULT 0 NOT NULL,
    "year" integer DEFAULT 0 NOT NULL,
    "testcode" character varying(50) NOT NULL,
    "notests" double precision DEFAULT 0,
    "unitprice" double precision DEFAULT 0
);


--
-- Name: tlkpcommenttopics; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpcommenttopics" (
    "topic" character varying(25) NOT NULL
);


--
-- Name: tlkpfrequency; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpfrequency" (
    "frequencyid" integer NOT NULL,
    "frequency" character varying(50)
);


--
-- Name: tlkpmilestonetype; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpmilestonetype" (
    "idtype" character(1) NOT NULL,
    "type" character varying(50),
    "milestonedeliverable" character(1)
);


--
-- Name: tlkpmonths; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpmonths" (
    "fmonthno" integer NOT NULL,
    "monthno" integer,
    "monthname" character varying(50)
);


--
-- Name: tlkpprojectstatus; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpprojectstatus" (
    "projectstatus" character varying(50) NOT NULL,
    "is_fps" boolean NOT NULL,
    "is_pims" boolean NOT NULL
);


--
-- Name: tlkppublicationtype; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkppublicationtype" (
    "type" character varying(3) NOT NULL,
    "description" character varying(50)
);


--
-- Name: tlkpreviewitem; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpreviewitem" (
    "itemid" integer NOT NULL,
    "item" character varying(50)
);


--
-- Name: tlkprisk; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkprisk" (
    "riskid" integer NOT NULL,
    "riskrating" character varying(15) NOT NULL
);


--
-- Name: tlkpyear; Type: TABLE; Schema: mabarchive; Owner: -
--

CREATE TABLE "mabarchive"."tlkpyear" (
    "year" integer NOT NULL,
    "latestmonthreleased" integer
);


--
-- Name: vasuprojectlist; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vasuprojectlist" AS
 SELECT "my_tlkpproject_all"."year",
    "my_tlkpproject_all"."parentproject",
    "my_tlkpproject_all"."program",
    "g_tlkpproject"."projecttitle",
    "my_tlkpproject_all"."isdefraproject"
   FROM ("mabarchive"."my_tlkpproject_all"
     LEFT JOIN "mabarchive"."g_tlkpproject" ON ((("my_tlkpproject_all"."parentproject")::"text" = ("g_tlkpproject"."parentproject")::"text")))
  WHERE (((EXTRACT(month FROM CURRENT_DATE) = ANY (ARRAY[(1)::numeric, (2)::numeric, (3)::numeric])) AND (("my_tlkpproject_all"."year")::numeric >= (EXTRACT(year FROM CURRENT_DATE) - (1)::numeric))) OR ((EXTRACT(month FROM CURRENT_DATE) = ANY (ARRAY[(4)::numeric, (5)::numeric, (6)::numeric, (7)::numeric, (8)::numeric, (9)::numeric, (10)::numeric, (11)::numeric, (12)::numeric])) AND (("my_tlkpproject_all"."year")::numeric >= EXTRACT(year FROM CURRENT_DATE))));


--
-- Name: vmy_projectcustincome; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vmy_projectcustincome" AS
 SELECT COALESCE("pims"."year", "fps"."year") AS "year",
    COALESCE("pims"."project", "fps"."parentproject") AS "project",
    COALESCE("pims"."pybudget", "fps"."custincome") AS "custinc"
   FROM ("mabarchive"."my_fpsyeartotals" "fps"
     FULL JOIN "mabarchive"."my_tlkpprojectradtrackdata" "pims" ON ((("fps"."year" = "pims"."year") AND (("fps"."parentproject")::"text" = ("pims"."project")::"text"))));


--
-- Name: vg_tlkpprojectincome; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vg_tlkpprojectincome" AS
 SELECT "vmy_projectcustincome"."project",
    COALESCE("g_tlkpproject_radtrackdata"."overallcustincome", "sum"("vmy_projectcustincome"."custinc")) AS "totalprojectvalue"
   FROM ("mabarchive"."vmy_projectcustincome"
     LEFT JOIN "mabarchive"."g_tlkpproject_radtrackdata" ON ((("vmy_projectcustincome"."project")::"text" = ("g_tlkpproject_radtrackdata"."parentproject")::"text")))
  GROUP BY "vmy_projectcustincome"."project", "g_tlkpproject_radtrackdata"."overallcustincome";


--
-- Name: vlatestmonthyear; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vlatestmonthyear" AS
 SELECT "tlkpyear"."year",
    "tlkpyear"."latestmonthreleased",
        CASE
            WHEN ("tlkpyear"."latestmonthreleased" = 1) THEN ('April '::"text" || (("tlkpyear"."year")::character(4))::"text")
            WHEN ("tlkpyear"."latestmonthreleased" < 10) THEN ((('April - '::"text" || ("tlkpmonths"."monthname")::"text") || ' '::"text") || (("tlkpyear"."year")::character(4))::"text")
            ELSE ((((('April '::"text" || (("tlkpyear"."year")::character(4))::"text") || ' - '::"text") || ("tlkpmonths"."monthname")::"text") || ' '::"text") || ((("tlkpyear"."year" + 1))::character(4))::"text")
        END AS "period"
   FROM ("mabarchive"."tlkpyear"
     JOIN "mabarchive"."tlkpmonths" ON (("tlkpyear"."latestmonthreleased" = "tlkpmonths"."fmonthno")))
  WHERE ("tlkpyear"."year" = ( SELECT "max"("tlkpyear_1"."year") AS "expr1"
           FROM "mabarchive"."tlkpyear" "tlkpyear_1"
          WHERE ("tlkpyear_1"."latestmonthreleased" > 0)));


--
-- Name: vcurrent_projectinfo; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vcurrent_projectinfo" AS
 SELECT "my_tlkpproject"."year",
    "my_tlkpproject"."parentproject",
    "g_tlkpproject"."projecttitle",
    "g_tlkpproject"."costbookno",
    "g_tlkpproject"."disease",
    "g_tlkpproject"."contract",
    "g_tlkpproject"."shorttitle",
    "g_tlkpproject"."projectstatus",
    "my_tlkpproject"."program",
    "my_tlkpproject"."customer",
    "my_tlkpproject"."manager",
    "my_tlkpproject"."transferincome",
    "my_tlkpproject"."custincome",
    "my_tlkpproject"."wip_eoy",
    "my_tlkpproject"."wip_limit",
    "my_tlkpproject"."wip_current",
    "my_tlkpproject"."datecreated",
    "my_tlkpproject"."feccost",
    "my_tlkpproject"."profit",
    "my_tlkpproject"."budget_cvl",
    "my_tlkpproject"."caseworksub",
    "my_tlkpproject"."pvsincome",
    "my_tlkpproject"."plancaseworkdebit",
    "my_tlkpproject"."source",
    "my_tlkpprojectradtrackdata"."bfbudget",
    "my_tlkpprojectradtrackdata"."pybudget",
    "my_tlkpprojectradtrackdata"."seedcorn",
    "my_tlkpprojectradtrackdata"."manhours",
    "my_tlkpprojectradtrackdata"."mandays",
    "my_tlkpprojectradtrackdata"."manyears",
    "my_tlkpprojectradtrackdata"."paycosts",
    "my_tlkpprojectradtrackdata"."nonpayohcosts",
    "my_tlkpprojectradtrackdata"."testcosts",
    "my_tlkpprojectradtrackdata"."animalcosts",
    "my_tlkpprojectradtrackdata"."nonanimalcosts",
    "my_tlkpprojectradtrackdata"."manhourschanged",
    "my_tlkpprojectradtrackdata"."paycostschanged",
    "my_tlkpprojectradtrackdata"."nonpayohcostschanged",
    "my_tlkpprojectradtrackdata"."testcostschanged",
    "my_tlkpprojectradtrackdata"."animalcostschanged",
    "my_tlkpprojectradtrackdata"."nonanimalcostschanged",
    "my_tlkpprojectradtrackdata"."adjustment",
    "my_tlkpprojectradtrackdata"."adjustmentcomment",
    "my_tlkpprojectradtrackdata"."locked",
    "my_tlkpprojectradtrackdata"."datecosted",
    "my_tlkpprojectradtrackdata"."costedby",
    "my_tlkpprojectradtrackdata"."actualexpenditure",
    "my_tlkpprojectradtrackdata"."actualmanyears",
    "my_tlkpprojectradtrackdata"."vla_budget",
    "vg_tlkpprojectincome"."totalprojectvalue",
    "my_tlkpproject"."projectgroup"
   FROM (((("mabarchive"."my_tlkpproject"
     JOIN "mabarchive"."vlatestmonthyear" ON (("my_tlkpproject"."year" = "vlatestmonthyear"."year")))
     JOIN "mabarchive"."g_tlkpproject" ON ((("my_tlkpproject"."parentproject")::"text" = ("g_tlkpproject"."parentproject")::"text")))
     JOIN "mabarchive"."vg_tlkpprojectincome" ON ((("g_tlkpproject"."parentproject")::"text" = ("vg_tlkpprojectincome"."project")::"text")))
     LEFT JOIN "mabarchive"."my_tlkpprojectradtrackdata" ON ((("my_tlkpproject"."year" = "my_tlkpprojectradtrackdata"."year") AND (("my_tlkpproject"."parentproject")::"text" = ("my_tlkpprojectradtrackdata"."project")::"text"))));


--
-- Name: vcurrent_tlkpprojectradtrackdata; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vcurrent_tlkpprojectradtrackdata" AS
 SELECT "my_tlkpprojectradtrackdata"."year",
    "my_tlkpprojectradtrackdata"."project",
    "my_tlkpprojectradtrackdata"."bfbudget",
    "my_tlkpprojectradtrackdata"."pybudget",
    "my_tlkpprojectradtrackdata"."seedcorn",
    "my_tlkpprojectradtrackdata"."manhours",
    "my_tlkpprojectradtrackdata"."mandays",
    "my_tlkpprojectradtrackdata"."manyears",
    "my_tlkpprojectradtrackdata"."paycosts",
    "my_tlkpprojectradtrackdata"."nonpayohcosts",
    "my_tlkpprojectradtrackdata"."testcosts",
    "my_tlkpprojectradtrackdata"."animalcosts",
    "my_tlkpprojectradtrackdata"."nonanimalcosts",
    "my_tlkpprojectradtrackdata"."manhourschanged",
    "my_tlkpprojectradtrackdata"."paycostschanged",
    "my_tlkpprojectradtrackdata"."nonpayohcostschanged",
    "my_tlkpprojectradtrackdata"."testcostschanged",
    "my_tlkpprojectradtrackdata"."animalcostschanged",
    "my_tlkpprojectradtrackdata"."nonanimalcostschanged",
    "my_tlkpprojectradtrackdata"."adjustment",
    "my_tlkpprojectradtrackdata"."adjustmentcomment",
    "my_tlkpprojectradtrackdata"."locked",
    "my_tlkpprojectradtrackdata"."datecosted",
    "my_tlkpprojectradtrackdata"."costedby",
    "my_tlkpprojectradtrackdata"."actualexpenditure",
    "my_tlkpprojectradtrackdata"."actualmanyears",
    "my_tlkpprojectradtrackdata"."vla_budget"
   FROM ("mabarchive"."my_tlkpprojectradtrackdata"
     JOIN "mabarchive"."vlatestmonthyear" ON (("my_tlkpprojectradtrackdata"."year" = "vlatestmonthyear"."year")));


--
-- Name: vfiveyearprojectsummary_sub; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vfiveyearprojectsummary_sub" AS
 SELECT "my_projectmonthfinal"."year",
    "my_projectmonthfinal"."project",
    "my_tlkpproject"."custincome" AS "cumbudget",
    "sum"("my_projectmonthfinal"."totalcost") AS "cumcost"
   FROM ("mabarchive"."my_projectmonthfinal"
     JOIN "mabarchive"."my_tlkpproject" ON ((("my_projectmonthfinal"."year" = "my_tlkpproject"."year") AND (("my_projectmonthfinal"."project")::"text" = ("my_tlkpproject"."parentproject")::"text"))))
  GROUP BY "my_projectmonthfinal"."year", "my_projectmonthfinal"."project", "my_tlkpproject"."custincome"
 HAVING ("my_projectmonthfinal"."year" >= 2004);


--
-- Name: vfiveyearprojectsummary_sub2; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vfiveyearprojectsummary_sub2" AS
 SELECT "my_tlkpproject"."parentproject" AS "project",
    "my_tlkpproject"."year",
    (((("my_tlkpproject"."year")::character(4))::"text" || '/'::"text") || "right"(((("my_tlkpproject"."year" + 1))::character(4))::"text", 2)) AS "displayyear",
    "my_tlkpproject"."custincome",
    "my_projectmonthfinal"."cumcost" AS "vlaexpeniture",
    ("my_tlkpproject"."custincome" - "my_projectmonthfinal"."cumcost") AS "incomelesscost",
    "my_projectmonthfinal"."cuminvoices" AS "invoicedincome",
    ("my_projectmonthfinal"."cuminvoices" - "my_projectmonthfinal"."cumcost") AS "invoiceslesscost",
    "my_tlkpproject"."budget_cvl" AS "budget",
    ("my_tlkpproject"."budget_cvl" - "my_projectmonthfinal"."cumcost") AS "budgetremaining"
   FROM ((("mabarchive"."my_projectmonthfinal"
     JOIN "mabarchive"."my_tlkpproject" ON ((("my_projectmonthfinal"."year" = "my_tlkpproject"."year") AND (("my_projectmonthfinal"."project")::"text" = ("my_tlkpproject"."parentproject")::"text"))))
     JOIN ( SELECT "my_projectmonthfinal_1"."year",
            "max"("my_projectmonthfinal_1"."monthno") AS "latestmonth"
           FROM "mabarchive"."my_projectmonthfinal" "my_projectmonthfinal_1"
          WHERE ("my_projectmonthfinal_1"."cumflag" = (1)::double precision)
          GROUP BY "my_projectmonthfinal_1"."year") "l" ON ((("my_projectmonthfinal"."year" = "l"."year") AND ("my_projectmonthfinal"."monthno" = "l"."latestmonth"))))
     CROSS JOIN "mabarchive"."vlatestmonthyear")
  WHERE (("my_tlkpproject"."year" >= ("vlatestmonthyear"."year" - 5)) AND ("my_tlkpproject"."year" <=
        CASE
            WHEN ("right"(("my_tlkpproject"."program")::"text", 4) = '_Res'::"text") THEN ("vlatestmonthyear"."year" - 1)
            WHEN ("right"(("my_tlkpproject"."program")::"text", 5) = '_SURV'::"text") THEN ("vlatestmonthyear"."year" - 1)
            WHEN (("my_tlkpproject"."program")::"text" = 'OM_WORK'::"text") THEN ("vlatestmonthyear"."year" - 1)
            ELSE "vlatestmonthyear"."year"
        END));


--
-- Name: vfiveyearprojectsummary; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vfiveyearprojectsummary" AS
 SELECT "sub2"."project",
    "sub2"."year",
    "sub2"."displayyear",
    "sub2"."custincome",
    "sub2"."vlaexpeniture",
    "sub2"."incomelesscost",
    "sub2"."invoicedincome",
    "sub2"."invoiceslesscost",
    "sub2"."budget",
    "sub2"."budgetremaining",
    "sum"("sub"."cumbudget") AS "cumbudget",
    "sum"("sub"."cumcost") AS "cumcost"
   FROM ("mabarchive"."vfiveyearprojectsummary_sub2" "sub2"
     JOIN "mabarchive"."vfiveyearprojectsummary_sub" "sub" ON (((("sub2"."project")::"text" = ("sub"."project")::"text") AND ("sub2"."year" >= "sub"."year"))))
  GROUP BY "sub2"."project", "sub2"."year", "sub2"."displayyear", "sub2"."custincome", "sub2"."vlaexpeniture", "sub2"."incomelesscost", "sub2"."invoicedincome", "sub2"."invoiceslesscost", "sub2"."budget", "sub2"."budgetremaining";


--
-- Name: vg_tlkpproject; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vg_tlkpproject" AS
 SELECT "g_tlkpproject"."parentproject",
    "g_tlkpproject"."projecttitle",
    "g_tlkpproject"."costbookno",
    "g_tlkpproject"."disease",
    "g_tlkpproject"."contract",
    "g_tlkpproject"."shorttitle",
    "g_tlkpproject"."projectstatus",
    "vcurrent_tlkpprojectradtrackdata"."bfbudget" AS "currentbfbudget"
   FROM ("mabarchive"."g_tlkpproject"
     LEFT JOIN "mabarchive"."vcurrent_tlkpprojectradtrackdata" ON ((("g_tlkpproject"."parentproject")::"text" = ("vcurrent_tlkpprojectradtrackdata"."project")::"text")));


--
-- Name: vlatestprojectyear; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vlatestprojectyear" AS
 SELECT "parentproject",
    "max"("year") AS "year"
   FROM "mabarchive"."my_tlkpproject"
  GROUP BY "parentproject";


--
-- Name: vmilestonesforcurrentfy; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vmilestonesforcurrentfy" AS
 SELECT "tblmilestone"."project",
    "tblmilestone"."number",
    "tblmilestone"."description",
    "tblmilestone"."datedue",
    "tblmilestone"."datecompleted",
    "tblmilestone"."dateformreceived",
    "tblmilestone"."undersdreview",
    "tblmilestone"."ontarget",
    "tblmilestone"."projectleadercomment",
    "tblmilestone"."capscomment",
    "tblmilestone"."idtype",
        CASE
            WHEN (("vlatestmonthyear"."year")::double precision = "date_part"('year'::"text", ("tblmilestone"."datedue" - '3 mons'::interval))) THEN '-1'::integer
            ELSE 0
        END AS "inthisfyear"
   FROM ("mabarchive"."tblmilestone"
     CROSS JOIN "mabarchive"."vlatestmonthyear")
  WHERE (("date_part"('year'::"text", ("tblmilestone"."datedue" - '3 mons'::interval)) = ("vlatestmonthyear"."year")::double precision) OR ("date_part"('year'::"text", ("tblmilestone"."datedue" - '6 mons'::interval)) = ("vlatestmonthyear"."year")::double precision));


--
-- Name: vmy_projectanimalplan; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vmy_projectanimalplan" AS
 SELECT "my_tlkpproject"."year",
    "my_tlkpproject"."parentproject",
    "my_tblanimalreq"."animaltype",
    "my_tblanimalreq"."numberofdays",
    "my_tblanimalreq"."numberofanimals",
        CASE
            WHEN (("my_tlkpproject"."isdefraproject" <> 0) AND ("my_tlkpproject"."year" >= 2013)) THEN "my_tblanimals"."defradailyrate"
            ELSE "my_tblanimals"."dailyrate"
        END AS "rate",
    ((
        CASE
            WHEN (("my_tlkpproject"."isdefraproject" <> 0) AND ("my_tlkpproject"."year" >= 2013)) THEN "my_tblanimals"."defradailyrate"
            ELSE "my_tblanimals"."dailyrate"
        END * "my_tblanimalreq"."numberofdays") * "my_tblanimalreq"."numberofanimals") AS "cost"
   FROM (("mabarchive"."my_tlkpproject"
     JOIN "mabarchive"."my_tblanimalreq" ON ((("my_tlkpproject"."year" = "my_tblanimalreq"."year") AND (("my_tlkpproject"."parentproject")::"text" = ("my_tblanimalreq"."jobcode")::"text"))))
     JOIN "mabarchive"."my_tblanimals" ON ((("my_tblanimalreq"."year" = "my_tblanimals"."year") AND (("my_tblanimalreq"."animaltype")::"text" = ("my_tblanimals"."animaltype")::"text"))));


--
-- Name: vmy_projectstaffplan; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vmy_projectstaffplan" AS
 SELECT "my_tlkpproject"."year",
    "my_tlkpproject"."parentproject",
    "my_profitcentregrade"."pcgrade",
    "my_staff"."workgroupgrade",
    "my_staff"."name",
    "my_tblstaffjob"."plannedhours",
        CASE
            WHEN (("my_tlkpproject"."isdefraproject" <> 0) AND ("my_tlkpproject"."year" >= 2013)) THEN ("my_profitcentregrade"."npr" + "my_profitcentregrade"."payrate")
            ELSE "my_profitcentregrade"."chargerate"
        END AS "rate",
        CASE
            WHEN (("my_tlkpproject"."isdefraproject" <> 0) AND ("my_tlkpproject"."year" >= 2013)) THEN ("my_tblstaffjob"."plannedhours" * ("my_profitcentregrade"."npr" + "my_profitcentregrade"."payrate"))
            ELSE ("my_tblstaffjob"."plannedhours" * "my_profitcentregrade"."chargerate")
        END AS "cost"
   FROM (((("mabarchive"."my_tlkpproject"
     JOIN "mabarchive"."my_tblstaffjob" ON ((("my_tlkpproject"."year" = "my_tblstaffjob"."year") AND (("my_tlkpproject"."parentproject")::"text" = ("my_tblstaffjob"."jobcode")::"text"))))
     JOIN "mabarchive"."my_staff" ON ((("my_tblstaffjob"."year" = "my_staff"."year") AND (("my_tblstaffjob"."staffid")::"text" = ("my_staff"."staffid")::"text"))))
     JOIN "mabarchive"."my_workgroupgrade" ON ((("my_staff"."year" = "my_workgroupgrade"."year") AND (("my_staff"."workgroupgrade")::"text" = ("my_workgroupgrade"."wggrade")::"text"))))
     JOIN "mabarchive"."my_profitcentregrade" ON ((("my_workgroupgrade"."year" = "my_profitcentregrade"."year") AND (("my_workgroupgrade"."profitcentregrade")::"text" = ("my_profitcentregrade"."pcgrade")::"text"))));


--
-- Name: vmy_radtrack_reports_forfyandnext; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vmy_radtrack_reports_forfyandnext" AS
 SELECT "my_radtrack_reports"."year",
    "my_radtrack_reports"."project",
    "my_radtrack_reports"."type",
    "my_radtrack_reports"."reminder1",
    "my_radtrack_reports"."reminder2",
    "my_radtrack_reports"."replyreceived",
    "my_radtrack_reports"."senttoprogmanager",
    "my_radtrack_reports"."senttoprojleader",
    "my_radtrack_reports"."emailedtocustomer",
    "my_radtrack_reports"."signedcopytocustomer",
    "my_radtrack_reports"."repduedate",
    "my_radtrack_reports"."id",
        CASE
            WHEN ("my_radtrack_reports"."emailedtocustomer" IS NULL) THEN NULL::"text"
            WHEN ("my_radtrack_reports"."repduedate" IS NULL) THEN NULL::"text"
            WHEN ("my_radtrack_reports"."emailedtocustomer" <= "my_radtrack_reports"."repduedate") THEN 'Yes'::"text"
            ELSE 'No'::"text"
        END AS "ontime"
   FROM ("mabarchive"."vlatestmonthyear"
     CROSS JOIN "mabarchive"."my_radtrack_reports")
  WHERE (("vlatestmonthyear"."year" = "my_radtrack_reports"."year") OR (("vlatestmonthyear"."year" + 1) = "my_radtrack_reports"."year"));


--
-- Name: vtcc_summary; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vtcc_summary" AS
 SELECT "year",
    "project",
    "month",
    "sum"("pay") AS "pay",
    "sum"("nonpay") AS "nonpay",
    "sum"("overhead") AS "overhead"
   FROM "mabarchive"."my_timecostcalcs"
  GROUP BY "year", "project", "month";


--
-- Name: vpactprojectyearcosts; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vpactprojectyearcosts" AS
 SELECT "my_projectmonthfinal"."project",
        CASE "g_tlkpproject_radtrackdata"."useprojectyear"
            WHEN '-1'::integer THEN "date_part"('year'::"text", ("date_trunc"('month'::"text", ("g_tlkpproject_radtrackdata"."startdate")::timestamp with time zone) + ((((("my_projectmonthfinal"."monthno" + (3)::double precision) - "date_part"('month'::"text", "g_tlkpproject_radtrackdata"."startdate")))::integer)::double precision * '1 mon'::interval)))
            ELSE ("my_projectmonthfinal"."year")::double precision
        END AS "year",
    "my_projectmonthfinal"."monthno",
    "sum"("my_projectmonthfinal"."subcontracts") AS "subcontracts",
    "sum"("my_projectmonthfinal"."animals") AS "animals",
    "sum"("my_projectmonthfinal"."transfercosts") AS "tests",
    "sum"("vtcc_summary"."pay") AS "pay",
    "sum"(("vtcc_summary"."nonpay" + "vtcc_summary"."overhead")) AS "nonpayoh",
    "sum"("my_projectmonthfinal"."totalhours") AS "hours",
    "sum"("my_projectmonthfinal"."totalcost") AS "totalcosts",
    "sum"("my_projectmonthfinal"."timecosts") AS "timecost"
   FROM (("mabarchive"."my_projectmonthfinal"
     LEFT JOIN "mabarchive"."g_tlkpproject_radtrackdata" ON ((("my_projectmonthfinal"."project")::"text" = ("g_tlkpproject_radtrackdata"."parentproject")::"text")))
     LEFT JOIN "mabarchive"."vtcc_summary" ON ((("my_projectmonthfinal"."year" = "vtcc_summary"."year") AND (("my_projectmonthfinal"."project")::"text" = ("vtcc_summary"."project")::"text") AND ("my_projectmonthfinal"."monthno" = "vtcc_summary"."month"))))
  GROUP BY "my_projectmonthfinal"."project", "my_projectmonthfinal"."monthno", "g_tlkpproject_radtrackdata"."useprojectyear", "my_projectmonthfinal"."year", "g_tlkpproject_radtrackdata"."startdate";


--
-- Name: vprogramreports_mail; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vprogramreports_mail" AS
 SELECT (((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("sq"."program")::"text") || '_'::"text") || ("prepname"."setting")::"text") || '">'::"text") || ("sq"."program")::"text") || '</a><br> '::"text") AS "hlink",
        CASE
            WHEN (("tblprojectmanager"."projectmanager")::"text" ~~ '%,%'::"text") THEN (((SUBSTRING("tblprojectmanager"."projectmanager" FROM (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) + 2) FOR 50) || ' '::"text") || "left"(("tblprojectmanager"."projectmanager")::"text", (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) - 1))))::character varying
            ELSE "tblprojectmanager"."projectmanager"
        END AS "projectmanager",
    "tblprojectmanager"."mnumber",
    "tblprojectmanager"."email",
    "sq"."program",
    "sq"."year",
    "tblprojectmanager"."disable"
   FROM ((((("mabarchive"."tblprogram_manager_link"
     JOIN "mabarchive"."tblradtrackprog" ON ((("tblprogram_manager_link"."program")::"text" = ("tblradtrackprog"."program")::"text")))
     JOIN ( SELECT "vcurrent_projectinfo"."year",
                CASE
                    WHEN (("vcurrent_projectinfo"."projectgroup")::"text" = 'SCN_RES'::"text") THEN "vcurrent_projectinfo"."projectgroup"
                    ELSE "vcurrent_projectinfo"."program"
                END AS "program"
           FROM "mabarchive"."vcurrent_projectinfo"
          WHERE (("vcurrent_projectinfo"."projectstatus")::"text" <> 'Completed'::"text")) "sq" ON ((("tblradtrackprog"."program")::"text" = ("sq"."program")::"text")))
     JOIN "mabarchive"."tblprojectmanager" ON ((("tblprogram_manager_link"."manager")::"text" = ("tblprojectmanager"."projectmanager")::"text")))
     CROSS JOIN "mabarchive"."tbl_settings" "root")
     CROSS JOIN "mabarchive"."tbl_settings" "prepname")
  WHERE ((("root"."id")::"text" = 'PIMS_Program_Current_Root'::"text") AND (("prepname"."id")::"text" = 'PIMS_Program_Report_Name'::"text") AND ("tblradtrackprog"."radtrackprog" = true))
  GROUP BY (((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("sq"."program")::"text") || '_'::"text") || ("prepname"."setting")::"text") || '">'::"text") || ("sq"."program")::"text") || '</a><br> '::"text"), "tblprojectmanager"."mnumber", "tblprojectmanager"."email", "sq"."program", "sq"."year", "tblprojectmanager"."projectmanager", "tblprojectmanager"."disable"
 HAVING (("sq"."program")::"text" <> 'ZT_Prog'::"text");


--
-- Name: vprojectlatestdetails; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vprojectlatestdetails" AS
 SELECT "g_tlkpproject"."parentproject",
    "my_tlkpproject"."program",
    "my_tlkpproject"."manager",
    "g_tlkpproject"."projecttitle",
    "g_tlkpproject"."shorttitle",
    "my_tlkpproject"."customer",
    "vlatestprojectyear"."year" AS "lastyear",
        CASE
            WHEN ("vlatestprojectyear"."year" = ( SELECT "max"("tlkpyear"."year") AS "max"
               FROM "mabarchive"."tlkpyear")) THEN 'Y'::"text"
            ELSE 'N'::"text"
        END AS "active",
    "my_tlkpproject"."projectgroup"
   FROM (("mabarchive"."g_tlkpproject"
     JOIN "mabarchive"."vlatestprojectyear" ON ((("g_tlkpproject"."parentproject")::"text" = ("vlatestprojectyear"."parentproject")::"text")))
     JOIN "mabarchive"."my_tlkpproject" ON ((("my_tlkpproject"."year" = "vlatestprojectyear"."year") AND (("my_tlkpproject"."parentproject")::"text" = ("vlatestprojectyear"."parentproject")::"text"))));


--
-- Name: vprojectreports_pmmail; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vprojectreports_pmmail" AS
 SELECT (((((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("vcurrent_projectinfo"."projectgroup")::"text") || '/'::"text") || "replace"(("vcurrent_projectinfo"."parentproject")::"text", '/'::"text", '-'::"text")) || ' '::"text") || ("prepname"."setting")::"text") || '" target="_blank">'::"text") || ("vcurrent_projectinfo"."parentproject")::"text") || '</a><br> '::"text") AS "hlink",
        CASE
            WHEN (("tblprojectmanager"."projectmanager")::"text" ~~ '%,%'::"text") THEN (((SUBSTRING("tblprojectmanager"."projectmanager" FROM (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) + 2) FOR 50) || ' '::"text") || "left"(("tblprojectmanager"."projectmanager")::"text", (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) - 1))))::character varying
            ELSE "tblprojectmanager"."projectmanager"
        END AS "projectmanager",
    "tblprojectmanager"."mnumber",
    "vcurrent_projectinfo"."parentproject",
    "tblprojectmanager"."email",
    ((((('<a href="'::"text" || ("editroot"."setting")::"text") || ("vcurrent_projectinfo"."parentproject")::"text") || '">'::"text") || ("vcurrent_projectinfo"."parentproject")::"text") || '</a><br>'::"text") AS "editlink",
    "vcurrent_projectinfo"."projectgroup",
    "vcurrent_projectinfo"."year",
    "tblprojectmanager"."disable"
   FROM (((("mabarchive"."tblradtrackprog"
     JOIN ("mabarchive"."tblprojectmanager"
     JOIN "mabarchive"."vcurrent_projectinfo" ON ((("tblprojectmanager"."projectmanager")::"text" = ("vcurrent_projectinfo"."manager")::"text"))) ON ((("tblradtrackprog"."program")::"text" = ("vcurrent_projectinfo"."program")::"text")))
     CROSS JOIN "mabarchive"."tbl_settings" "prepname")
     CROSS JOIN "mabarchive"."tbl_settings" "root")
     CROSS JOIN "mabarchive"."tbl_settings" "editroot")
  WHERE ((("root"."id")::"text" = 'PIMS_Project_Current_Root'::"text") AND (("prepname"."id")::"text" = 'PIMS_Project_Report_Name'::"text") AND (("editroot"."id")::"text" = 'PIMS_Project_Edit_Link'::"text") AND ("tblradtrackprog"."radtrackprog" = true) AND (("vcurrent_projectinfo"."projectstatus")::"text" <> 'Completed'::"text") AND (("vcurrent_projectinfo"."projectgroup")::"text" <> ALL (ARRAY[('EU_PROG'::character varying)::"text", ('ZT_Prog'::character varying)::"text"])));


--
-- Name: vprojectreports_pmmilestoneemail; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vprojectreports_pmmilestoneemail" AS
 SELECT "hlink",
    "projectmanager",
    "mnumber",
    "parentproject",
    "email",
    "editlink",
    "year",
    "disable"
   FROM "mabarchive"."vprojectreports_pmmail"
  WHERE (EXISTS ( SELECT "tblmilestone"."project",
            "tblmilestone"."number",
            "tblmilestone"."description",
            "tblmilestone"."datedue",
            "tblmilestone"."datecompleted",
            "tblmilestone"."dateformreceived",
            "tblmilestone"."undersdreview",
            "tblmilestone"."ontarget",
            "tblmilestone"."projectleadercomment",
            "tblmilestone"."capscomment",
            "tblmilestone"."idtype"
           FROM "mabarchive"."tblmilestone"
          WHERE ((("vprojectreports_pmmail"."parentproject")::"text" = ("tblmilestone"."project")::"text") AND (("vprojectreports_pmmail"."year")::double precision = "date_part"('year'::"text", "tblmilestone"."datedue")))));


--
-- Name: vprojectreports_pmmilestonemail; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vprojectreports_pmmilestonemail" AS
 SELECT "hlink",
    "projectmanager",
    "mnumber",
    "parentproject",
    "email",
    "editlink",
    "year",
    "disable"
   FROM "mabarchive"."vprojectreports_pmmail"
  WHERE (EXISTS ( SELECT "tblmilestone"."project",
            "tblmilestone"."number",
            "tblmilestone"."description",
            "tblmilestone"."datedue",
            "tblmilestone"."datecompleted",
            "tblmilestone"."dateformreceived",
            "tblmilestone"."undersdreview",
            "tblmilestone"."ontarget",
            "tblmilestone"."projectleadercomment",
            "tblmilestone"."capscomment",
            "tblmilestone"."idtype"
           FROM "mabarchive"."tblmilestone"
          WHERE ((("vprojectreports_pmmail"."parentproject")::"text" = ("tblmilestone"."project")::"text") AND (("vprojectreports_pmmail"."year")::numeric = EXTRACT(year FROM "tblmilestone"."datedue")))));


--
-- Name: vprojectreports_programmmail; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vprojectreports_programmmail" AS
 SELECT (((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("sq"."program")::"text") || '_'::"text") || ("prepname"."setting")::"text") || '">'::"text") || ("sq"."program")::"text") || '</a><br> '::"text") AS "hlink",
        CASE
            WHEN (("tblprojectmanager"."projectmanager")::"text" ~~ '%,%'::"text") THEN (((SUBSTRING("tblprojectmanager"."projectmanager" FROM (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) + 2) FOR 50) || ' '::"text") || "left"(("tblprojectmanager"."projectmanager")::"text", (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) - 1))))::character varying
            ELSE "tblprojectmanager"."projectmanager"
        END AS "projectmanager",
    "tblprojectmanager"."mnumber",
    "tblprojectmanager"."email",
    "sq"."program",
    "sq"."year",
    "tblprojectmanager"."disable"
   FROM ((((("mabarchive"."tblprogram_manager_link"
     JOIN "mabarchive"."tblradtrackprog" ON ((("tblprogram_manager_link"."program")::"text" = ("tblradtrackprog"."program")::"text")))
     JOIN ( SELECT "vcurrent_projectinfo"."year",
                CASE
                    WHEN (("vcurrent_projectinfo"."projectgroup")::"text" = 'SCN_RES'::"text") THEN "vcurrent_projectinfo"."projectgroup"
                    ELSE "vcurrent_projectinfo"."program"
                END AS "program"
           FROM "mabarchive"."vcurrent_projectinfo"
          WHERE (("vcurrent_projectinfo"."projectstatus")::"text" <> 'Completed'::"text")) "sq" ON ((("tblradtrackprog"."program")::"text" = ("sq"."program")::"text")))
     JOIN "mabarchive"."tblprojectmanager" ON ((("tblprogram_manager_link"."manager")::"text" = ("tblprojectmanager"."projectmanager")::"text")))
     CROSS JOIN "mabarchive"."tbl_settings" "root")
     CROSS JOIN "mabarchive"."tbl_settings" "prepname")
  WHERE ((("prepname"."id")::"text" = 'PIMS_Program_Report_Name'::"text") AND ("tblradtrackprog"."radtrackprog" = true) AND (("root"."id")::"text" = 'PIMS_Program_Current_Root'::"text"))
  GROUP BY (((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("sq"."program")::"text") || '_'::"text") || ("prepname"."setting")::"text") || '">'::"text") || ("sq"."program")::"text") || '</a><br> '::"text"), "tblprojectmanager"."mnumber", "tblprojectmanager"."email", "sq"."program", "sq"."year", "tblprojectmanager"."projectmanager", "tblprojectmanager"."disable"
 HAVING (("sq"."program")::"text" <> 'ZT_Prog'::"text");


--
-- Name: vrcreports_mail; Type: VIEW; Schema: mabarchive; Owner: -
--

CREATE VIEW "mabarchive"."vrcreports_mail" AS
 SELECT "my_tblprofitcentre"."profitcentre",
    (((((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("my_tblprofitcentre"."profitcentre")::"text") || ' '::"text") || ("rcrepname1"."setting")::"text") || '">'::"text") || ("my_tblprofitcentre"."profitcentre")::"text") || ' '::"text") || ("rcrepname1"."setting")::"text") || '</a><br> '::"text") AS "hlink1",
    (((((((((('<a href="'::"text" || ("root"."setting")::"text") || '/'::"text") || ("my_tblprofitcentre"."profitcentre")::"text") || ' '::"text") || ("rcrepname2"."setting")::"text") || '">'::"text") || ("my_tblprofitcentre"."profitcentre")::"text") || ' '::"text") || ("rcrepname2"."setting")::"text") || '</a><br> '::"text") AS "hlink2",
        CASE
            WHEN (("tblprojectmanager"."projectmanager")::"text" ~~ '%,%'::"text") THEN (((SUBSTRING("tblprojectmanager"."projectmanager" FROM (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) + 2) FOR 50) || ' '::"text") || SUBSTRING("tblprojectmanager"."projectmanager" FROM 1 FOR (POSITION((','::"text") IN ("tblprojectmanager"."projectmanager")) - 1))))::character varying
            ELSE "tblprojectmanager"."projectmanager"
        END AS "projectmanager",
    "tblprojectmanager"."mnumber",
    "tblprojectmanager"."email",
    "tblprojectmanager"."disable"
   FROM (((((("mabarchive"."vlatestmonthyear"
     JOIN "mabarchive"."my_tblprofitcentre" ON (("vlatestmonthyear"."year" = "my_tblprofitcentre"."year")))
     JOIN "mabarchive"."tblprofitcentre_manager_link" ON ((("my_tblprofitcentre"."profitcentre")::"text" = ("tblprofitcentre_manager_link"."profitcentre")::"text")))
     JOIN "mabarchive"."tblprojectmanager" ON ((("tblprofitcentre_manager_link"."manager")::"text" = ("tblprojectmanager"."projectmanager")::"text")))
     CROSS JOIN "mabarchive"."tbl_settings" "rcrepname1")
     CROSS JOIN "mabarchive"."tbl_settings" "rcrepname2")
     CROSS JOIN "mabarchive"."tbl_settings" "root")
  WHERE ((("root"."id")::"text" = 'PIMS_RC_Current_Root'::"text") AND (("rcrepname1"."id")::"text" = 'PIMS_RC_Report_Name1'::"text") AND (("rcrepname2"."id")::"text" = 'PIMS_RC_Report_Name2'::"text"));


--
-- Name: g_tlkpproject pk_g_tlkpproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."g_tlkpproject"
    ADD CONSTRAINT "pk_g_tlkpproject" PRIMARY KEY ("parentproject");


--
-- Name: g_tlkpproject_radtrackdata pk_g_tlkpproject_radtrackdata; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."g_tlkpproject_radtrackdata"
    ADD CONSTRAINT "pk_g_tlkpproject_radtrackdata" PRIMARY KEY ("parentproject");


--
-- Name: my_fpsyeartotals pk_my_fpsyeartotals; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_fpsyeartotals"
    ADD CONSTRAINT "pk_my_fpsyeartotals" PRIMARY KEY ("year", "parentproject");


--
-- Name: my_milestoneformdates pk_my_milestoneformdates; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_milestoneformdates"
    ADD CONSTRAINT "pk_my_milestoneformdates" PRIMARY KEY ("year", "parentproject");


--
-- Name: my_monthlyoutput pk_my_monthlyoutput; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_monthlyoutput"
    ADD CONSTRAINT "pk_my_monthlyoutput" PRIMARY KEY ("year", "testcode", "buyer", "month", "workgroup");


--
-- Name: my_monthlytime pk_my_monthlytime; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_monthlytime"
    ADD CONSTRAINT "pk_my_monthlytime" PRIMARY KEY ("year", "pactstaffid", "timecode", "month", "parentproject");


--
-- Name: my_profitcentregrade pk_my_profitcentregrade; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_profitcentregrade"
    ADD CONSTRAINT "pk_my_profitcentregrade" PRIMARY KEY ("year", "pcgrade");


--
-- Name: my_proj_invoice pk_my_proj_invoice; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_proj_invoice"
    ADD CONSTRAINT "pk_my_proj_invoice" PRIMARY KEY ("year", "projectparent", "invoicecounter");


--
-- Name: my_proj_subcontract pk_my_proj_subcontract; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_proj_subcontract"
    ADD CONSTRAINT "pk_my_proj_subcontract" PRIMARY KEY ("year", "subcontcounter");


--
-- Name: my_projectmonthfinal pk_my_projectmonthfinal; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_projectmonthfinal"
    ADD CONSTRAINT "pk_my_projectmonthfinal" PRIMARY KEY ("year", "project", "monthno");


--
-- Name: my_radtrack_reports pk_my_radtrack_reports; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_radtrack_reports"
    ADD CONSTRAINT "pk_my_radtrack_reports" PRIMARY KEY ("id");


--
-- Name: my_staff pk_my_staff; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_staff"
    ADD CONSTRAINT "pk_my_staff" PRIMARY KEY ("year", "staffid");


--
-- Name: my_tbladditionalcosts pk_my_tbladditionalcosts; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tbladditionalcosts"
    ADD CONSTRAINT "pk_my_tbladditionalcosts" PRIMARY KEY ("ac_counter");


--
-- Name: my_tblanimalreq pk_my_tblanimalreq; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tblanimalreq"
    ADD CONSTRAINT "pk_my_tblanimalreq" PRIMARY KEY ("ar_counter");


--
-- Name: my_tblanimals pk_my_tblanimals; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tblanimals"
    ADD CONSTRAINT "pk_my_tblanimals" PRIMARY KEY ("year", "animaltype");


--
-- Name: my_tblcontract pk_my_tblcontract; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tblcontract"
    ADD CONSTRAINT "pk_my_tblcontract" PRIMARY KEY ("year", "contractno");


--
-- Name: my_tblprofitcentre pk_my_tblprofitcentre; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tblprofitcentre"
    ADD CONSTRAINT "pk_my_tblprofitcentre" PRIMARY KEY ("year", "profitcentre");


--
-- Name: my_tblstaffjob pk_my_tblstaffjob; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tblstaffjob"
    ADD CONSTRAINT "pk_my_tblstaffjob" PRIMARY KEY ("year", "staffid", "jobcode");


--
-- Name: my_testorproduct pk_my_testorproduct; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_testorproduct"
    ADD CONSTRAINT "pk_my_testorproduct" PRIMARY KEY ("year", "itemcode");


--
-- Name: my_timecostcalcs pk_my_timecostcalcs; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_timecostcalcs"
    ADD CONSTRAINT "pk_my_timecostcalcs" PRIMARY KEY ("year", "workgroup", "jobcode", "project", "month", "staffid");


--
-- Name: my_tlkpprogram pk_my_tlkpprogram; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tlkpprogram"
    ADD CONSTRAINT "pk_my_tlkpprogram" PRIMARY KEY ("year", "programno");


--
-- Name: my_tlkpproject pk_my_tlkpproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tlkpproject"
    ADD CONSTRAINT "pk_my_tlkpproject" PRIMARY KEY ("year", "parentproject");


--
-- Name: my_tlkpproject_all pk_my_tlkpproject_all; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tlkpproject_all"
    ADD CONSTRAINT "pk_my_tlkpproject_all" PRIMARY KEY ("year", "parentproject");


--
-- Name: my_tlkpprojectradtrackdata pk_my_tlkpprojectradtrackdata; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tlkpprojectradtrackdata"
    ADD CONSTRAINT "pk_my_tlkpprojectradtrackdata" PRIMARY KEY ("year", "project");


--
-- Name: my_tlkptestreqmt pk_my_tlkptestreqmt; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tlkptestreqmt"
    ADD CONSTRAINT "pk_my_tlkptestreqmt" PRIMARY KEY ("year", "testcode", "buyer");


--
-- Name: my_workgroup pk_my_workgroup; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_workgroup"
    ADD CONSTRAINT "pk_my_workgroup" PRIMARY KEY ("year", "workgroup");


--
-- Name: my_workgroupgrade pk_my_workgroupgrade; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_workgroupgrade"
    ADD CONSTRAINT "pk_my_workgroupgrade" PRIMARY KEY ("year", "wggrade");


--
-- Name: tbl_settings pk_tbl_settings; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbl_settings"
    ADD CONSTRAINT "pk_tbl_settings" PRIMARY KEY ("id");


--
-- Name: tblaccesslevels pk_tblaccesslevels; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccesslevels"
    ADD CONSTRAINT "pk_tblaccesslevels" PRIMARY KEY ("systemid", "accesslevelid");


--
-- Name: tblaccessprograms pk_tblaccessprograms; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessprograms"
    ADD CONSTRAINT "pk_tblaccessprograms" PRIMARY KEY ("systemid", "ntlogin", "program");


--
-- Name: tblaccesssystems pk_tblaccesssystems; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccesssystems"
    ADD CONSTRAINT "pk_tblaccesssystems" PRIMARY KEY ("systemid");


--
-- Name: tblaccessusers pk_tblaccessusers; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessusers"
    ADD CONSTRAINT "pk_tblaccessusers" PRIMARY KEY ("systemid", "ntlogin");


--
-- Name: tblaccessusers_levels pk_tblaccessusers_levels; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessusers_levels"
    ADD CONSTRAINT "pk_tblaccessusers_levels" PRIMARY KEY ("systemid", "ntlogin", "accesslevelid");


--
-- Name: tbladditionalcosts pk_tbladditionalcosts; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbladditionalcosts"
    ADD CONSTRAINT "pk_tbladditionalcosts" PRIMARY KEY ("ac_identity");


--
-- Name: tblanimalreq pk_tblanimalreq; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblanimalreq"
    ADD CONSTRAINT "pk_tblanimalreq" PRIMARY KEY ("ar_identity");


--
-- Name: tblcapsstaff pk_tblcapsstaff; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblcapsstaff"
    ADD CONSTRAINT "pk_tblcapsstaff" PRIMARY KEY ("mnumber");


--
-- Name: tblcomments pk_tblcomments; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblcomments"
    ADD CONSTRAINT "pk_tblcomments" PRIMARY KEY ("commentno");


--
-- Name: tblcsg7_accountgroups pk_tblcsg7_accountgroups; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblcsg7_accountgroups"
    ADD CONSTRAINT "pk_tblcsg7_accountgroups" PRIMARY KEY ("csg7group");


--
-- Name: tbldbvariables pk_tbldbvariables; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbldbvariables"
    ADD CONSTRAINT "pk_tbldbvariables" PRIMARY KEY ("db_variable");


--
-- Name: tbldisease pk_tbldisease; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbldisease"
    ADD CONSTRAINT "pk_tbldisease" PRIMARY KEY ("disease");


--
-- Name: tbleugrade_conversion pk_tbleugrade_conversion; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbleugrade_conversion"
    ADD CONSTRAINT "pk_tbleugrade_conversion" PRIMARY KEY ("vlagrade");


--
-- Name: tblfpsyearstoimport pk_tblfpsyearstoimport; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblfpsyearstoimport"
    ADD CONSTRAINT "pk_tblfpsyearstoimport" PRIMARY KEY ("fpsname");


--
-- Name: tblimages pk_tblimages; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblimages"
    ADD CONSTRAINT "pk_tblimages" PRIMARY KEY ("imageid");


--
-- Name: tbllogmilestone pk_tbllogmilestone; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbllogmilestone"
    ADD CONSTRAINT "pk_tbllogmilestone" PRIMARY KEY ("id");


--
-- Name: tblmaintenance pk_tblmaintenance; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblmaintenance"
    ADD CONSTRAINT "pk_tblmaintenance" PRIMARY KEY ("formname");


--
-- Name: tblmilestone pk_tblmilestone; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblmilestone"
    ADD CONSTRAINT "pk_tblmilestone" PRIMARY KEY ("project", "number");


--
-- Name: tblprofitcentre_manager_link pk_tblprofitcentre_manager_link; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprofitcentre_manager_link"
    ADD CONSTRAINT "pk_tblprofitcentre_manager_link" PRIMARY KEY ("profitcentre", "manager");


--
-- Name: tblprogram_manager_link pk_tblprogram_manager_link; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprogram_manager_link"
    ADD CONSTRAINT "pk_tblprogram_manager_link" PRIMARY KEY ("program", "manager");


--
-- Name: tblproject pk_tblproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblproject"
    ADD CONSTRAINT "pk_tblproject" PRIMARY KEY ("project");


--
-- Name: tblprojectmanager pk_tblprojectmanager; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprojectmanager"
    ADD CONSTRAINT "pk_tblprojectmanager" PRIMARY KEY ("projectmanager");


--
-- Name: tblprojectreviewitems pk_tblprojectreviewitems; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprojectreviewitems"
    ADD CONSTRAINT "pk_tblprojectreviewitems" PRIMARY KEY ("project", "itemid");


--
-- Name: tblprojectyear pk_tblprojectyear; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprojectyear"
    ADD CONSTRAINT "pk_tblprojectyear" PRIMARY KEY ("project", "yearno");


--
-- Name: tblproposedproject pk_tblproposedproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblproposedproject"
    ADD CONSTRAINT "pk_tblproposedproject" PRIMARY KEY ("id");


--
-- Name: tblpublication pk_tblpublication; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblpublication"
    ADD CONSTRAINT "pk_tblpublication" PRIMARY KEY ("uid");


--
-- Name: tblpublicationproject pk_tblpublicationproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblpublicationproject"
    ADD CONSTRAINT "pk_tblpublicationproject" PRIMARY KEY ("publicationuid", "parentproject");


--
-- Name: tblradtrackcontract pk_tblradtrackcontract; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblradtrackcontract"
    ADD CONSTRAINT "pk_tblradtrackcontract" PRIMARY KEY ("contract");


--
-- Name: tblradtrackinvoice pk_tblradtrackinvoice; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblradtrackinvoice"
    ADD CONSTRAINT "pk_tblradtrackinvoice" PRIMARY KEY ("invoicecounter");


--
-- Name: tblradtrackprog pk_tblradtrackprog; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblradtrackprog"
    ADD CONSTRAINT "pk_tblradtrackprog" PRIMARY KEY ("program");


--
-- Name: tblreport pk_tblreport; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblreport"
    ADD CONSTRAINT "pk_tblreport" PRIMARY KEY ("id");


--
-- Name: tblreportgroup pk_tblreportgroup; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblreportgroup"
    ADD CONSTRAINT "pk_tblreportgroup" PRIMARY KEY ("groupid");


--
-- Name: tblreportgroup_link pk_tblreportgroup_link; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblreportgroup_link"
    ADD CONSTRAINT "pk_tblreportgroup_link" PRIMARY KEY ("reportid", "groupid");


--
-- Name: tblstaffrequ pk_tblstaffrequ; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblstaffrequ"
    ADD CONSTRAINT "pk_tblstaffrequ" PRIMARY KEY ("sr_identity");


--
-- Name: tbltestrequ pk_tbltestrequ; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbltestrequ"
    ADD CONSTRAINT "pk_tbltestrequ" PRIMARY KEY ("project", "year", "testcode");


--
-- Name: temptbladditionalcosts pk_temptbladditionalcosts; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptbladditionalcosts"
    ADD CONSTRAINT "pk_temptbladditionalcosts" PRIMARY KEY ("ac_identity");


--
-- Name: temptblanimalreq pk_temptblanimalreq; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblanimalreq"
    ADD CONSTRAINT "pk_temptblanimalreq" PRIMARY KEY ("ar_identity");


--
-- Name: temptblproject pk_temptblproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblproject"
    ADD CONSTRAINT "pk_temptblproject" PRIMARY KEY ("project");


--
-- Name: temptblprojectyear pk_temptblprojectyear; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblprojectyear"
    ADD CONSTRAINT "pk_temptblprojectyear" PRIMARY KEY ("project", "yearno");


--
-- Name: temptblstaffrequ pk_temptblstaffrequ; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblstaffrequ"
    ADD CONSTRAINT "pk_temptblstaffrequ" PRIMARY KEY ("sr_identity");


--
-- Name: temptbltestreq pk_temptbltestreq; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptbltestreq"
    ADD CONSTRAINT "pk_temptbltestreq" PRIMARY KEY ("project", "year", "testcode");


--
-- Name: tlkpcommenttopics pk_tlkpcommenttopics; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpcommenttopics"
    ADD CONSTRAINT "pk_tlkpcommenttopics" PRIMARY KEY ("topic");


--
-- Name: tlkpfrequency pk_tlkpfrequency; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpfrequency"
    ADD CONSTRAINT "pk_tlkpfrequency" PRIMARY KEY ("frequencyid");


--
-- Name: tlkpmilestonetype pk_tlkpmilestonetype; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpmilestonetype"
    ADD CONSTRAINT "pk_tlkpmilestonetype" PRIMARY KEY ("idtype");


--
-- Name: tlkpmonths pk_tlkpmonths; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpmonths"
    ADD CONSTRAINT "pk_tlkpmonths" PRIMARY KEY ("fmonthno");


--
-- Name: tlkpprojectstatus pk_tlkpprojectstatus; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpprojectstatus"
    ADD CONSTRAINT "pk_tlkpprojectstatus" PRIMARY KEY ("projectstatus");


--
-- Name: tlkppublicationtype pk_tlkppublicationtype; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkppublicationtype"
    ADD CONSTRAINT "pk_tlkppublicationtype" PRIMARY KEY ("type");


--
-- Name: tlkpreviewitem pk_tlkpreviewitem; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpreviewitem"
    ADD CONSTRAINT "pk_tlkpreviewitem" PRIMARY KEY ("itemid");


--
-- Name: tlkprisk pk_tlkprisk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkprisk"
    ADD CONSTRAINT "pk_tlkprisk" PRIMARY KEY ("riskid");


--
-- Name: tlkpyear pk_tlkpyear; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tlkpyear"
    ADD CONSTRAINT "pk_tlkpyear" PRIMARY KEY ("year");


--
-- Name: tblproposedproject uq_tblproposedproject_parentproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblproposedproject"
    ADD CONSTRAINT "uq_tblproposedproject_parentproject" UNIQUE ("parentproject");


--
-- Name: idx_my_monthlyoutput_month; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_my_monthlyoutput_month" ON "mabarchive"."my_monthlyoutput" USING "btree" ("month");


--
-- Name: idx_my_monthlyoutput_testcode; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_my_monthlyoutput_testcode" ON "mabarchive"."my_monthlyoutput" USING "btree" ("testcode");


--
-- Name: idx_my_monthlyoutput_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_my_monthlyoutput_year" ON "mabarchive"."my_monthlyoutput" USING "btree" ("year");


--
-- Name: idx_my_tlkpproject_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_my_tlkpproject_year" ON "mabarchive"."my_tlkpproject" USING "btree" ("year");


--
-- Name: idx_tbl_settings_id; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tbl_settings_id" ON "mabarchive"."tbl_settings" USING "btree" ("id");


--
-- Name: idx_tbladditionalcosts_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tbladditionalcosts_project" ON "mabarchive"."tbladditionalcosts" USING "btree" ("project");


--
-- Name: idx_tbladditionalcosts_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tbladditionalcosts_project_year" ON "mabarchive"."tbladditionalcosts" USING "btree" ("project", "year");


--
-- Name: idx_tblanimalreq_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tblanimalreq_project" ON "mabarchive"."tblanimalreq" USING "btree" ("project");


--
-- Name: idx_tblanimalreq_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tblanimalreq_project_year" ON "mabarchive"."tblanimalreq" USING "btree" ("project", "year");


--
-- Name: idx_tblanimalreq_project_year_animaltype; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tblanimalreq_project_year_animaltype" ON "mabarchive"."tblanimalreq" USING "btree" ("project", "year", "animaltype");


--
-- Name: idx_tblprojectyear_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tblprojectyear_project" ON "mabarchive"."tblprojectyear" USING "btree" ("project");


--
-- Name: idx_tblstaffrequ_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tblstaffrequ_project" ON "mabarchive"."tblstaffrequ" USING "btree" ("project");


--
-- Name: idx_tblstaffrequ_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tblstaffrequ_project_year" ON "mabarchive"."tblstaffrequ" USING "btree" ("project", "year");


--
-- Name: idx_tbltestrequ_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tbltestrequ_project" ON "mabarchive"."tbltestrequ" USING "btree" ("project");


--
-- Name: idx_tbltestrequ_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_tbltestrequ_project_year" ON "mabarchive"."tbltestrequ" USING "btree" ("project", "year");


--
-- Name: idx_temptbladditionalcosts_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptbladditionalcosts_project" ON "mabarchive"."temptbladditionalcosts" USING "btree" ("project");


--
-- Name: idx_temptbladditionalcosts_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptbladditionalcosts_project_year" ON "mabarchive"."temptbladditionalcosts" USING "btree" ("project", "year");


--
-- Name: idx_temptblanimalreq_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptblanimalreq_project" ON "mabarchive"."temptblanimalreq" USING "btree" ("project");


--
-- Name: idx_temptblanimalreq_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptblanimalreq_project_year" ON "mabarchive"."temptblanimalreq" USING "btree" ("project", "year");


--
-- Name: idx_temptblanimalreq_project_year_animaltype; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptblanimalreq_project_year_animaltype" ON "mabarchive"."temptblanimalreq" USING "btree" ("project", "year", "animaltype");


--
-- Name: idx_temptblprojectyear_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptblprojectyear_project" ON "mabarchive"."temptblprojectyear" USING "btree" ("project");


--
-- Name: idx_temptblstaffrequ_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptblstaffrequ_project" ON "mabarchive"."temptblstaffrequ" USING "btree" ("project");


--
-- Name: idx_temptblstaffrequ_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptblstaffrequ_project_year" ON "mabarchive"."temptblstaffrequ" USING "btree" ("project", "year");


--
-- Name: idx_temptbltestreq_project; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptbltestreq_project" ON "mabarchive"."temptbltestreq" USING "btree" ("project");


--
-- Name: idx_temptbltestreq_project_year; Type: INDEX; Schema: mabarchive; Owner: -
--

CREATE INDEX "idx_temptbltestreq_project_year" ON "mabarchive"."temptbltestreq" USING "btree" ("project", "year");


--
-- Name: g_tlkpproject_radtrackdata fk_g_tlkpproject_radtrackdata_tlkprisk; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."g_tlkpproject_radtrackdata"
    ADD CONSTRAINT "fk_g_tlkpproject_radtrackdata_tlkprisk" FOREIGN KEY ("riskid") REFERENCES "mabarchive"."tlkprisk"("riskid");


--
-- Name: my_milestoneformdates fk_my_milestoneformdates_g_tlkpproject_radtrackdata; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_milestoneformdates"
    ADD CONSTRAINT "fk_my_milestoneformdates_g_tlkpproject_radtrackdata" FOREIGN KEY ("parentproject") REFERENCES "mabarchive"."g_tlkpproject_radtrackdata"("parentproject");


--
-- Name: my_radtrack_reports fk_my_radtrack_reports_g_tlkpproject_radtrackdata; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_radtrack_reports"
    ADD CONSTRAINT "fk_my_radtrack_reports_g_tlkpproject_radtrackdata" FOREIGN KEY ("project") REFERENCES "mabarchive"."g_tlkpproject_radtrackdata"("parentproject");


--
-- Name: my_tlkpprojectradtrackdata fk_my_tlkpprojectradtrackdata_g_tlkpproject_radtrackdata; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."my_tlkpprojectradtrackdata"
    ADD CONSTRAINT "fk_my_tlkpprojectradtrackdata_g_tlkpproject_radtrackdata" FOREIGN KEY ("project") REFERENCES "mabarchive"."g_tlkpproject_radtrackdata"("parentproject");


--
-- Name: tblaccesslevels fk_tblaccesslevels_tblaccesssystems; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccesslevels"
    ADD CONSTRAINT "fk_tblaccesslevels_tblaccesssystems" FOREIGN KEY ("systemid") REFERENCES "mabarchive"."tblaccesssystems"("systemid");


--
-- Name: tblaccessprograms fk_tblaccessprograms_tblaccessusers; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessprograms"
    ADD CONSTRAINT "fk_tblaccessprograms_tblaccessusers" FOREIGN KEY ("systemid", "ntlogin") REFERENCES "mabarchive"."tblaccessusers"("systemid", "ntlogin");


--
-- Name: tblaccessprograms fk_tblaccessprograms_tblradtrackprog; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessprograms"
    ADD CONSTRAINT "fk_tblaccessprograms_tblradtrackprog" FOREIGN KEY ("program") REFERENCES "mabarchive"."tblradtrackprog"("program");


--
-- Name: tblaccessusers_levels fk_tblaccessusers_levels_tblaccesslevels; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessusers_levels"
    ADD CONSTRAINT "fk_tblaccessusers_levels_tblaccesslevels" FOREIGN KEY ("systemid", "accesslevelid") REFERENCES "mabarchive"."tblaccesslevels"("systemid", "accesslevelid");


--
-- Name: tblaccessusers_levels fk_tblaccessusers_levels_tblaccessusers; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessusers_levels"
    ADD CONSTRAINT "fk_tblaccessusers_levels_tblaccessusers" FOREIGN KEY ("systemid", "ntlogin") REFERENCES "mabarchive"."tblaccessusers"("systemid", "ntlogin");


--
-- Name: tblaccessusers fk_tblaccessusers_tblaccesssystems; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblaccessusers"
    ADD CONSTRAINT "fk_tblaccessusers_tblaccesssystems" FOREIGN KEY ("systemid") REFERENCES "mabarchive"."tblaccesssystems"("systemid");


--
-- Name: tbladditionalcosts fk_tbladditionalcosts_tblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbladditionalcosts"
    ADD CONSTRAINT "fk_tbladditionalcosts_tblprojectyear" FOREIGN KEY ("year", "project") REFERENCES "mabarchive"."tblprojectyear"("yearno", "project");


--
-- Name: tblanimalreq fk_tblanimalreq_tblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblanimalreq"
    ADD CONSTRAINT "fk_tblanimalreq_tblprojectyear" FOREIGN KEY ("project", "year") REFERENCES "mabarchive"."tblprojectyear"("project", "yearno");


--
-- Name: tblcomments fk_tblcomments_tlkpcommenttopics; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblcomments"
    ADD CONSTRAINT "fk_tblcomments_tlkpcommenttopics" FOREIGN KEY ("topic") REFERENCES "mabarchive"."tlkpcommenttopics"("topic");


--
-- Name: tblmilestone fk_tblmilestone_g_tlkpproject_radtrackdata; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblmilestone"
    ADD CONSTRAINT "fk_tblmilestone_g_tlkpproject_radtrackdata" FOREIGN KEY ("project") REFERENCES "mabarchive"."g_tlkpproject_radtrackdata"("parentproject");


--
-- Name: tblmilestone fk_tblmilestone_tlkpmilestonetype; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblmilestone"
    ADD CONSTRAINT "fk_tblmilestone_tlkpmilestonetype" FOREIGN KEY ("idtype") REFERENCES "mabarchive"."tlkpmilestonetype"("idtype");


--
-- Name: tblprojectreviewitems fk_tblprojectreviewitems_tlkpfrequency; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprojectreviewitems"
    ADD CONSTRAINT "fk_tblprojectreviewitems_tlkpfrequency" FOREIGN KEY ("frequencyid") REFERENCES "mabarchive"."tlkpfrequency"("frequencyid");


--
-- Name: tblprojectreviewitems fk_tblprojectreviewitems_tlkpreviewitem; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprojectreviewitems"
    ADD CONSTRAINT "fk_tblprojectreviewitems_tlkpreviewitem" FOREIGN KEY ("itemid") REFERENCES "mabarchive"."tlkpreviewitem"("itemid");


--
-- Name: tblprojectyear fk_tblprojectyear_tblproject; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblprojectyear"
    ADD CONSTRAINT "fk_tblprojectyear_tblproject" FOREIGN KEY ("project") REFERENCES "mabarchive"."tblproject"("project");


--
-- Name: tblpublication fk_tblpublication_tlkppublicationtype; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblpublication"
    ADD CONSTRAINT "fk_tblpublication_tlkppublicationtype" FOREIGN KEY ("type") REFERENCES "mabarchive"."tlkppublicationtype"("type");


--
-- Name: tblpublicationproject fk_tblpublicationproject_tblpublication; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblpublicationproject"
    ADD CONSTRAINT "fk_tblpublicationproject_tblpublication" FOREIGN KEY ("publicationuid") REFERENCES "mabarchive"."tblpublication"("uid");


--
-- Name: tblradtrackinvoice fk_tblradtrackinvoice_g_tlkpproject_radtrackdata; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblradtrackinvoice"
    ADD CONSTRAINT "fk_tblradtrackinvoice_g_tlkpproject_radtrackdata" FOREIGN KEY ("project") REFERENCES "mabarchive"."g_tlkpproject_radtrackdata"("parentproject");


--
-- Name: tblradtrackinvoice fk_tblradtrackinvoice_tblradtrackcontract; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblradtrackinvoice"
    ADD CONSTRAINT "fk_tblradtrackinvoice_tblradtrackcontract" FOREIGN KEY ("contract") REFERENCES "mabarchive"."tblradtrackcontract"("contract");


--
-- Name: tblreportgroup_link fk_tblreportgroup_link_tblreportgroup; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblreportgroup_link"
    ADD CONSTRAINT "fk_tblreportgroup_link_tblreportgroup" FOREIGN KEY ("groupid") REFERENCES "mabarchive"."tblreportgroup"("groupid");


--
-- Name: tblstaffrequ fk_tblstaffrequ_tblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tblstaffrequ"
    ADD CONSTRAINT "fk_tblstaffrequ_tblprojectyear" FOREIGN KEY ("year", "project") REFERENCES "mabarchive"."tblprojectyear"("yearno", "project");


--
-- Name: tbltestrequ fk_tbltestrequ_tblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."tbltestrequ"
    ADD CONSTRAINT "fk_tbltestrequ_tblprojectyear" FOREIGN KEY ("year", "project") REFERENCES "mabarchive"."tblprojectyear"("yearno", "project");


--
-- Name: temptbladditionalcosts fk_temptbladditionalcosts_temptblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptbladditionalcosts"
    ADD CONSTRAINT "fk_temptbladditionalcosts_temptblprojectyear" FOREIGN KEY ("project", "year") REFERENCES "mabarchive"."temptblprojectyear"("project", "yearno");


--
-- Name: temptblanimalreq fk_temptblanimalreq_temptblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblanimalreq"
    ADD CONSTRAINT "fk_temptblanimalreq_temptblprojectyear" FOREIGN KEY ("year", "project") REFERENCES "mabarchive"."temptblprojectyear"("yearno", "project");


--
-- Name: temptblprojectyear fk_temptblprojectyear_temptblproject; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblprojectyear"
    ADD CONSTRAINT "fk_temptblprojectyear_temptblproject" FOREIGN KEY ("project") REFERENCES "mabarchive"."temptblproject"("project");


--
-- Name: temptblstaffrequ fk_temptblstaffrequ_temptblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptblstaffrequ"
    ADD CONSTRAINT "fk_temptblstaffrequ_temptblprojectyear" FOREIGN KEY ("project", "year") REFERENCES "mabarchive"."temptblprojectyear"("project", "yearno");


--
-- Name: temptbltestreq fk_temptbltestreq_temptblprojectyear; Type: FK CONSTRAINT; Schema: mabarchive; Owner: -
--

ALTER TABLE ONLY "mabarchive"."temptbltestreq"
    ADD CONSTRAINT "fk_temptbltestreq_temptblprojectyear" FOREIGN KEY ("year", "project") REFERENCES "mabarchive"."temptblprojectyear"("yearno", "project");



--
-- PostgreSQL database dump complete
--



--rollback empty
