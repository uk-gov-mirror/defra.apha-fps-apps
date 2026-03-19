-- Table: fps.proj_invoice

CREATE TABLE fps.proj_invoice (
    projectparent citext NOT NULL,
    month integer,
    amount money,
    costofwork money,
    wip money,
    profitloss money,
    detail character varying(100),
    invoicecounter integer NOT NULL,
    x character varying(5),
    type character varying(10),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_proj_invoice PRIMARY KEY (invoicecounter, fpsyear),
    CONSTRAINT proj_invoice_ck_proj_invoice_2__22 CHECK (type::text = 'PVSIncome'::text OR type::text = 'CVOGIncome'::text),
    CONSTRAINT fk_proj_invoice_projectparent FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_proj_invoice_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
