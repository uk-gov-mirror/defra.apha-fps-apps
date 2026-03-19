-- Table: fps.testreq_log

CREATE TABLE fps.testreq_log (
    sequenceno integer DEFAULT nextval('fps.testreq_log_sequenceno_seq'::regclass) NOT NULL,
    testcode character varying(20),
    buyer character varying(20),
    unitprice double precision,
    norequired integer,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    active smallint,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    jobcode character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_testreq_log PRIMARY KEY (sequenceno, fpsyear),

    CONSTRAINT fk_testreq_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
