-- View: fps.vworkgroupgrade

CREATE OR REPLACE VIEW fps.vworkgroupgrade AS
 SELECT wgg.wggrade,
    wgg.profitcentregrade,
    wgg.gradecode,
    wgg.workgroup,
    wgg.chargeratewg,
    wgg.directratewg,
    wgg.payratewg,
    wgg.nprwg,
    wgg.ohrwg,
    wgg.avsalary,
    wgg.hrschangedby,
    wgg.fpsyear
   FROM fps.workgroupgrade wgg
   JOIN fps.vworkgroup wg ON wg.workgroup = wgg.workgroup AND wg.fpsyear = wgg.fpsyear;
