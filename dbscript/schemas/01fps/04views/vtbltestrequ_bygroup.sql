-- View: fps.vtbltestrequ_bygroup

CREATE OR REPLACE VIEW fps.vtbltestrequ_bygroup AS
SELECT buyer AS jobcode,
    testcode,
    norequired AS notests,
    unitprice AS testprice,
    datecreated,
    projectbuyercode,
    fpsyear
   FROM fps.tlkptestreqmt
  WHERE ((buyer)::text IN ( SELECT vtlkpproject_bygroup.parentproject
           FROM fps.vtlkpproject_bygroup));
