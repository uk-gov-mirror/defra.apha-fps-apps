-- View: fps.vtbltestrequ_tm

CREATE OR REPLACE VIEW fps.vtbltestrequ_tm AS
SELECT buyer AS jobcode,
    testcode,
    norequired AS notests,
    unitprice AS testprice,
    fpsyear
   FROM fps.tlkptestreqmt;
