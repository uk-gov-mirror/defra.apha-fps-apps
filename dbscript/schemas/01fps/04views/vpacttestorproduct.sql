-- View: fps.vpacttestorproduct

CREATE OR REPLACE VIEW fps.vpacttestorproduct AS
 SELECT itemcode,
    itemdescription,
    shortdescription,
    testmanager,
    owner,
    jobstatus,
    unitpricevla AS unitpricevlagen,
    priceahvg AS priceahvgx,
    defraunitprice,
    fpsyear
   FROM fps.testorproduct;
