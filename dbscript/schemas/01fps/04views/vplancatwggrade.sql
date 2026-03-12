-- View: fps.vplancatwggrade

CREATE OR REPLACE VIEW fps.vplancatwggrade AS
 SELECT pcwg.plancategory,
    pcwg.wggrade,
    pcwg.hours,
    pcwg.createdby,
    pcwg.selleragrees,
    pcwg.buyeragrees,
    pcwg.fpsyear
   FROM fps.plancatwggrade pcwg
   JOIN fps.vworkgroupgrade wgg ON wgg.wggrade = pcwg.wggrade AND wgg.fpsyear = pcwg.fpsyear;
