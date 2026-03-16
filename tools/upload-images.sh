#!/usr/bin/env bash
# 批次上傳 picture/ 目錄的圖片到 Cloudinary
# 用法：tools/upload-images.sh <cloud_name> <api_key> <api_secret>
set -e

PICTURE_DIR="/Users/chendavid/side_project/two/picture"
CLOUD_NAME="$1"
API_KEY="$2"
API_SECRET="$3"

if [[ -z "$CLOUD_NAME" || -z "$API_KEY" || -z "$API_SECRET" ]]; then
    echo "Usage: $0 <cloud_name> <api_key> <api_secret>"
    exit 1
fi

SUCCESS=0
FAILED=0

while IFS= read -r filepath; do
    # 取相對路徑
    rel="${filepath#$PICTURE_DIR/}"

    # 拆路徑各層
    IFS='/' read -ra parts <<< "$rel"
    depth="${#parts[@]}"

    category="${parts[0]}"
    filename="${parts[$((depth - 1))]}"

    # 從檔名取數字（正面=1, 背面=2）
    sort_order=$(echo "$filename" | grep -oE '[0-9]+' | tail -1)
    [[ -z "$sort_order" ]] && sort_order=1

    # 產品名稱 = 圖片的上一層資料夾，去除前後空白
    product=$(echo "${parts[$((depth - 2))]}" | xargs)

    # 判斷是否有性別層（帽子沒有）
    if [[ "$category" == "帽子" ]]; then
        public_id="eshopx/${category}/${product}/${sort_order}"
    else
        gender="${parts[1]}"
        public_id="eshopx/${category}/${gender}/${product}/${sort_order}"
    fi

    # 產生 Cloudinary signature（SHA-1）
    timestamp=$(date +%s)
    sign_str="public_id=${public_id}&timestamp=${timestamp}${API_SECRET}"
    signature=$(echo -n "$sign_str" | shasum -a 1 | cut -d' ' -f1)

    echo -n "Uploading: $public_id ... "

    response=$(curl -s -X POST \
        "https://api.cloudinary.com/v1_1/${CLOUD_NAME}/image/upload" \
        -F "file=@${filepath}" \
        -F "public_id=${public_id}" \
        -F "api_key=${API_KEY}" \
        -F "timestamp=${timestamp}" \
        -F "signature=${signature}")

    if echo "$response" | python3 -c "import sys,json; d=json.load(sys.stdin); exit(0 if 'public_id' in d else 1)" 2>/dev/null; then
        echo "✓"
        SUCCESS=$((SUCCESS + 1))
    else
        error=$(echo "$response" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('error',{}).get('message','unknown'))" 2>/dev/null || echo "$response")
        echo "✗ $error"
        FAILED=$((FAILED + 1))
    fi

done < <(find "$PICTURE_DIR" -type f \( -name "*.jpg" -o -name "*.png" \) | sort)

echo ""
echo "完成：$SUCCESS 成功，$FAILED 失敗"
