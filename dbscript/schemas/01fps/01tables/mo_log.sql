-- Table: fps.mo_log

CREATE TABLE fps.mo_log (
    sequenceno integer GENERATED ALWAYS AS IDENTITY NOT NULL,
    testcode character varying(20),
    buyer character varying(20),
    month double precision,
    workgroup character varying(50),
    volume double precision,
    wgbuyer character varying(50),
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL,

    CONSTRAINT pk_mo_log PRIMARY KEY (sequenceno, fpsyear),

    CONSTRAINT fk_mo_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
