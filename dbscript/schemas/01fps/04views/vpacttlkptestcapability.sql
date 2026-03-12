-- View: fps.vpacttlkptestcapability

CREATE OR REPLACE VIEW fps.vpacttlkptestcapability AS
SELECT testcode,
    workgroup,
    planportfolio,
    smscode,
    ((testcode)::text || (workgroup)::text) AS wgtestcode,
    fpsyear
   FROM fps.tlkptestcapability;
