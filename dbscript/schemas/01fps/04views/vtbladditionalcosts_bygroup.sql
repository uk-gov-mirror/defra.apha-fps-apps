-- View: fps.vtbladditionalcosts_bygroup

CREATE OR REPLACE VIEW fps.vtbladditionalcosts_bygroup AS
 SELECT ac.jobcode,
    ac.account,
    ac.description,
    ac.itemcost,
    ac.freq,
    ac.supplier,
    ac.fpsyear
   FROM fps.tbladditionalcosts ac
   JOIN fps.vtlkpproject_bygroup p ON p.parentproject = ac.jobcode AND p.fpsyear = ac.fpsyear;
