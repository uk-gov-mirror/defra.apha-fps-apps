-- Table: fps.fpsyeartotals

CREATE TABLE fps.fpsyeartotals (
    parentproject character varying(20) NOT NULL,
    program character varying(10) NOT NULL,
    totaladditionalcosts money,
    totalanimalcosts double precision,
    totalstaffcosts double precision,
    totaltestcosts double precision,
    totalcosts double precision,
    custincome money NOT NULL,
    transferincome money NOT NULL,
    totalincome money NOT NULL,
    budget_cvl money,
    requiredprofit money,
    manager character varying(50),
    customer character varying(50),
    projectstatus character varying(50),
    pvsincome money,
    plancaseworkdebit money,
    totalpaycosts double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_fpsyeartotals PRIMARY KEY (parentproject, fpsyear),

    CONSTRAINT fk_fpsyeartotals_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
