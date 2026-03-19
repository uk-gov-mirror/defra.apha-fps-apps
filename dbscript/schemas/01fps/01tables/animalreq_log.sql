-- Table: fps.animalreq_log

CREATE TABLE fps.animalreq_log (
    sequenceno integer DEFAULT nextval('fps.animalreq_log_sequenceno_seq'::regclass) NOT NULL,
    jobcode character varying(20) NOT NULL,
    animaltype character varying(50) NOT NULL,
    numberofdays double precision NOT NULL,
    numberofanimals double precision NOT NULL,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_animalreq_log PRIMARY KEY (sequenceno, fpsyear),

    CONSTRAINT fk_animalreq_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
