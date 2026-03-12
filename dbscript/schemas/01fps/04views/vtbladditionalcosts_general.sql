-- View: fps.vtbladditionalcosts_general

CREATE OR REPLACE VIEW fps.vtbladditionalcosts_general AS
 SELECT jobcode,
    account,
    description,
    itemcost,
    freq,
    supplier,
    fpsyear
   FROM fps.tbladditionalcosts;
