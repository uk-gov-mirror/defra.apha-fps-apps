-- Table: fps.tlkpmanager

CREATE TABLE fps.tlkpmanager (
    manager character varying(50) NOT NULL,
    title character varying(10),
    workgroup character varying(50) NOT NULL,
    gradecode character varying(10) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpmanager PRIMARY KEY (manager, fpsyear),

    CONSTRAINT fk_tlkpmanager_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
