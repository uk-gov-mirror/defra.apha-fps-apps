-- View: fps.vcontracttestrequ

CREATE OR REPLACE VIEW fps.vcontracttestrequ AS
SELECT buyer AS jobcode,
    testcode,
    norequired AS notests,
    unitprice AS testprice,
    datecreated,
    projectbuyercode,
    fpsyear
   FROM fps.tlkptestreqmt
  WHERE ((buyer)::text IN ( SELECT vcontractproject.parentproject
           FROM fps.vcontractproject));
