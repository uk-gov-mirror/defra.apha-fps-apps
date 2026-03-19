-- Table: fps.staffjob_log

CREATE TABLE fps.staffjob_log (
    sequenceno integer DEFAULT nextval('fps.staffjob_log_sequenceno_seq'::regclass) NOT NULL,
    staffid character varying(50) NOT NULL,
    jobcode character varying(20) NOT NULL,
    plannedhours double precision NOT NULL,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_staffjob_log PRIMARY KEY (sequenceno, fpsyear),

    CONSTRAINT fk_staffjob_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
