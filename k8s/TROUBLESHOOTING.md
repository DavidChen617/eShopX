# Troubleshooting Notes (6-8)

## 6. Calico node 不 Ready（BGP/Typha）

- 現象：
  - `calico-node` 長時間 `0/1 Running`
  - log 出現 `BIRD is not ready` 或 `Failed to connect to Typha`
- 原因：
  - BGP 開啟但環境不需要（BIRD 啟動/連線失敗）
  - Typha 連線被安全群組擋住（TCP 5473）
- 解法：
  - 關閉 BGP：
    - `kubectl patch installation default --type=merge -p '{"spec":{"calicoNetwork":{"bgp":"Disabled"}}}'`
  - 安全群組開放 `TCP 5473`（同 SG 互通）

## 7. VXLAN CrossSubnet + BGP disabled → 跨節點 Pod 不通

- 現象：
  - `calico-node` 全部 Ready，但 Pod 跨節點 ping 不通
- 原因：
  - `vxlanMode: CrossSubnet` 在同子網不封裝
  - 又已關閉 BGP，缺乏路由宣告
- 解法：
  - 改成 VXLAN Always（在 Installation 設定 `encapsulation: VXLAN`）
  - 需在 `spec.calicoNetwork.ipPools[]` 補齊必要欄位（至少 `cidr`）

## 8. cert-manager 建立 ClusterIssuer 失敗

- 現象：
  - `no matches for kind "ClusterIssuer" in version "cert-manager.io/v1"`
  - 或 webhook 連線失敗
- 原因：
  - cert-manager CRD 未安裝
  - webhook 尚未 Ready
- 解法：
  - 先安裝 cert-manager：
    - `kubectl apply -f https://github.com/cert-manager/cert-manager/releases/latest/download/cert-manager.yaml`
  - 等 `cert-manager-webhook` Ready 後再建立 `ClusterIssuer`
