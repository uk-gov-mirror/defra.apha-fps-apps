-- View: fps.qryjobmonth_subcontracts1

CREATE OR REPLACE VIEW fps.qryjobmonth_subcontracts1 AS
 SELECT DISTINCT project,
    month,
    acctcode,
    fpsyear,
    sum((amount)::numeric) AS total,
        CASE
            WHEN ((acctcode)::text = ANY (ARRAY[('LargeAnimals'::character varying)::text, ('SmallAnimals'::character varying)::text, ('Mice'::character varying)::text])) THEN sum((amount)::numeric)
            ELSE (0)::numeric
        END AS animals1,
        CASE
            WHEN ((acctcode)::text = ANY (ARRAY[('LargeAnimals'::character varying)::text, ('SmallAnimals'::character varying)::text, ('Mice'::character varying)::text])) THEN (0)::numeric
            ELSE sum((amount)::numeric)
        END AS other1
   FROM fps.proj_subcontract
  GROUP BY project, month, acctcode, fpsyear;
