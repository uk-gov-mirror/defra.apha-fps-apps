-- Table: fps.tlkpjobcode

CREATE TABLE fps.tlkpjobcode (
    jobcode character varying(50) NOT NULL,
    parentproject citext,
    jobcodeworkgroup character varying(50),
    newprog character varying(20),
    type character varying(15),
    jobcodename character varying(255),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpjobcode PRIMARY KEY (jobcode, fpsyear),
    CONSTRAINT tlkpjobcode_ck_tlkpjobcode_1__11 CHECK (type IS NOT NULL),
    CONSTRAINT fk_tlkpjobcode_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear),

    CONSTRAINT fk_tlkpjobcode_parentproject FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear)
);
