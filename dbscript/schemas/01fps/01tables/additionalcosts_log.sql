-- Table: fps.additionalcosts_log

CREATE TABLE fps.additionalcosts_log (
    sequenceno integer DEFAULT nextval('fps.additionalcosts_log_sequenceno_seq'::regclass) NOT NULL,
    jobcode character varying(20) NOT NULL,
    account character varying(50) NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_additionalcosts_log PRIMARY KEY (sequenceno, fpsyear),

    CONSTRAINT fk_additionalcosts_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
