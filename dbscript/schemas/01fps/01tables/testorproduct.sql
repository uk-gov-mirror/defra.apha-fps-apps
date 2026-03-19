-- Table: fps.testorproduct

CREATE TABLE fps.testorproduct (
    itemcode citext NOT NULL,
    itemdescription character varying(200),
    testmanager character varying(50),
    jobstatus character varying(2),
    unitpricevla money DEFAULT 0,
    priceahvg money,
    owner character varying(2),
    chargemethod character varying(5),
    shortdescription character(18),
    defraunitprice money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_testorproduct PRIMARY KEY (itemcode, fpsyear),
    CONSTRAINT testorproduct_owner_cannot_be_null CHECK (owner IS NOT NULL AND (owner::text = 'PT'::text OR owner::text = 'PA'::text OR owner::text = 'SD'::text OR owner::text = 'LT'::text)),

    CONSTRAINT fk_testorproduct_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
