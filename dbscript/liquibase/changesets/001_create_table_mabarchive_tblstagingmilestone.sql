--liquibase formatted sql

--changeset repo-admin:001_mabarchive.tblstagingmilestone_table labels:ddl context:all
CREATE TABLE IF NOT EXISTS mabarchive.tblstagingmilestone
(
    id integer,
    project varchar(20),
    number varchar(10),
    description varchar(500),
    datedue timestamp without time zone NOT NULL,
    note text,
    alt_description text,
    alt_date text,
    alt_number text,
    typeid varchar(5),
	createdby varchar(255)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS mabarchive.tblstagingmilestone
    OWNER to fpsdev;

GRANT ALL ON TABLE mabarchive.tblstagingmilestone TO "FPSAdmin";

GRANT ALL ON TABLE mabarchive.tblstagingmilestone TO fpsdev;

--rollback DROP TABLE IF EXISTS public.department;
