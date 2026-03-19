-- Table: fps.recreatesummaries_log

CREATE TABLE fps.recreatesummaries_log (
    id integer DEFAULT nextval('fps.recreatesummaries_log_id_seq'::regclass) NOT NULL,
    userid character varying(20),
    period smallint,
    datedone timestamp without time zone,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_recreatesummaries_log PRIMARY KEY (id, fpsyear),

    CONSTRAINT fk_recreatesummaries_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
